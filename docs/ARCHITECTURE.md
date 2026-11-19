# Архитектура OrderEvents_Lite

Это карта проекта: как он устроен сейчас, кто кого вызывает, где проходят швы и почему решения приняты именно так. Документ рассчитан на разработчика, который открыл репозиторий и хочет понять структуру до чтения кода.

Принцип — Clean Architecture плюс event-driven in-process: домен поднимает события, Application оркестрирует команды и запросы через MediatR, Infrastructure реализует абстракции и доставляет события в фоне, API остаётся тонким HTTP-слоем. Запуск и эндпоинты описаны в [README.md](../README.md), осознанные упрощения — в [KNOWN_SIMPLIFICATIONS.md](../KNOWN_SIMPLIFICATIONS.md), план развития — в [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md).

Главная идея: изменения состояния фиксируются в домене и не блокируют HTTP-ответ; всё, что связано с доставкой и внешними эффектами, вынесено за границу транзакции. Ниже — слои, домен, Application, Infrastructure, API, полный пайплайн и точки расширения.

## Слои и зависимости

```text
PS.OrderEvents.API            Web API, контроллеры, DI, Program.cs
PS.OrderEvents.Application    CQRS, абстракции, хендлеры событий
PS.OrderEvents.Common         пусто, зарезервировано под логирование
PS.OrderEvents.Domain         Order, IDomainEvent, OrderCreated, OrderStatusChanged, OrderStatus
PS.OrderEvents.Infrastructure EF Core, SQLite, очередь, воркер, Telegram-заглушка

API            → Application, Infrastructure
Application    → Domain
Infrastructure → Application
Domain         → MediatR.Contracts (только интерфейсы, осознанно)
Common         — никто не ссылается
```

- **Domain** — агрегат `Order`, статусы и доменные события. Не знает ни про EF, ни про HTTP, ни про очередь; зависит только от `MediatR.Contracts`, чтобы события были `INotification`.
- **Application** — команды, запросы, их хендлеры, абстракции и хендлеры событий. Знает домен и абстракции, не знает реализаций.
- **Infrastructure** — EF Core/SQLite, репозиторий, UnitOfWork, in-process очередь, фоновый воркер и Telegram-заглушка. Реализует абстракции Application; домен про неё не знает.
- **API** — контроллеры и композиция DI. Тонкий слой, транслирующий HTTP в MediatR.
- **Common** — пуст и никем не используется, зарезервирован под логирование.

Ключевое отступление: `Domain → MediatR.Contracts` формально нарушает правило «домен ни от кого не зависит», но принято сознательно, чтобы не поддерживать собственный диспатчер. Подробнее — в [KNOWN_SIMPLIFICATIONS.md](../KNOWN_SIMPLIFICATIONS.md).

## Домен

`Order` — корень агрегата: `Id`, `Status`, `CreatedAt`, приватный конструктор для EF, фабрика `Create()` и метод `ChangeStatus(newStatus)` с guard (если статус тот же — событие не поднимается). Агрегат накапливает `List<IDomainEvent>`; наружу отдаёт `DomainEvents` (read-only) и `ClearDomainEvents()`.

`OrderStatus`: `Pending`, `Paid`, `Shipped`, `Completed`.

События — `record`-типы, реализующие `IDomainEvent`:

- `OrderCreated(Guid OrderId, DateTime OccurredOn)`;
- `OrderStatusChanged(Guid OrderId, OrderStatus OldStatus, OrderStatus NewStatus, DateTime OccurredOn)`.

`IDomainEvent : INotification { DateTime OccurredOn { get; } }`.

Ключевая мысль: событие поднимается в домене, в момент изменения состояния, а не в контроллере или хендлере команды. Поэтому инвариант «событие есть тогда и только тогда, когда состояние реально изменилось» живёт рядом с самим изменением.

## Application (CQRS)

Команды и запросы через MediatR:

- `CreateOrderCommand : IRequest<Guid>`;
- `ChangeOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest<Unit>`;
- `GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>`;
- `GetOrdersQuery : IRequest<IReadOnlyList<OrderDto>>`.

У каждой — свой хендлер. Абстракции: `IOrderRepository`, `IUnitOfWork`, `IDomainEventQueue`, `ITelegramNotifier`.

Хендлеры событий — все `INotificationHandler<TEvent>`: `OrderCreatedHandler`, `OrderStatusChangedHandler`, `TelegramOrderCreatedHandler`. Их находит MediatR при сканировании сборки Application.

Ключевая мысль: Application знает про домен и абстракции, но не про EF, HTTP или Telegram. Любая конкретная технология подставляется в Infrastructure/API.

## Infrastructure

- `OrderEventsDbContext` — EF Core поверх SQLite.
- `OrderRepository` — реализация `IOrderRepository`.
- `UnitOfWork` — реализация `IUnitOfWork`. Порядок: `context.SaveChangesAsync` (коммит) → собрать события из `ChangeTracker` → `queue.EnqueueAsync` по одному → `ClearDomainEvents`.
- `InMemoryDomainEventQueue` — `Channel<IDomainEvent>`, bounded 100, `FullMode.Wait`, `SingleReader = true`, `SingleWriter = false`.
- `DomainEventQueueProcessor : BackgroundService` — читает Channel, делает `Task.Delay(3000)` (demo delay), создаёт scope через `IServiceScopeFactory` и вызывает `IPublisher.Publish`.
- `TelegramNotifierStub` — реализация `ITelegramNotifier` с флагом `Telegram:SimulateFailure`.
- `DependencyInjection.AddInfrastructure(config)` — регистрация DbContext, репозитория, UnitOfWork, очереди, воркера и Telegram-заглушки.

Ключевая мысль: Infrastructure реализует абстракции Application, а домен про неё не знает. Замена SQLite, очереди или Telegram затрагивает этот слой, но не домен.

## API

`OrdersController` — четыре эндпоинта:

| Метод | Путь | Тело | Ответ |
|-------|------|------|-------|
| `POST` | `/orders` | — | `201 Created` |
| `PATCH` | `/orders/{id}/status` | `{ "newStatus": 1 }` | `204 No Content` |
| `GET` | `/orders/{id}` | — | `200` / `404` |
| `GET` | `/orders` | — | `200` |

`Program.cs`: `AddControllers`, `AddOpenApi`, `AddInfrastructure`, `AddMediatR(RegisterServicesFromAssembly(...))`, CORS для `localhost:5173` (http/https), `EnsureCreated` в scope при старте, `UseCors` до `UseHttpsRedirection`/`UseAuthorization`, `MapControllers`. `OrderStatus` в JSON передаётся числом (0..3).

Ключевая мысль: слой только принимает HTTP и передаёт его в MediatR; бизнес-логики здесь нет.

## Пайплайн целиком

```text
POST /orders
  → OrdersController.Create
  → mediator.Send(CreateOrderCommand)
  → CreateOrderCommandHandler
  → Order.Create()                    ← здесь поднимается OrderCreated (событие)
  → repository.AddAsync
  → unitOfWork.SaveChangesAsync
      → context.SaveChangesAsync      ← здесь коммит в БД
      → собрали DomainEvents из ChangeTracker
      → foreach → queue.EnqueueAsync  ← здесь событие попадает в Channel, не ждём
      → ClearDomainEvents
  → 201 Created (~250–300 мс)

[фон, DomainEventQueueProcessor]
  → Channel.Reader.ReadAllAsync        ← здесь начинается фон
  → Task.Delay(3000)                   ← demo delay
  → scope → IPublisher.Publish
      → INotificationHandler<OrderCreated>  → [EVENT]
      → TelegramOrderCreatedHandler         → [TELEGRAM]
  → try/catch: ошибка логируется, воркер живёт
```

Ответ приходит быстро, потому что запрос завершается после коммита и постановки событий в очередь; побочные эффекты выполняются в фоне. Это и есть контраст sync vs async: HTTP не ждёт Telegram и прочих обработчиков.

## Швы (точки расширения)

- **`IDomainEventQueue`** — замена Channel на RabbitMQ/Kafka. Меняются реализация очереди, `AddInfrastructure` и consumer; домен, хендлеры и `UnitOfWork` не трогаются.
- **`IUnitOfWork`** — внедрение Outbox. Меняются `UnitOfWork`, `DbContext` и добавляется фоновый publisher; домен и хендлеры не трогаются.
- **`ITelegramNotifier`** — реальная реализация Telegram. Заглушка заменяется на `TelegramNotifier` с `HttpClient` и секретами; хендлеры не трогаются.
- **`INotificationHandler<T>`** — добавление хендлера. Достаточно нового класса; MediatR находит его при сканировании сборки Application, очередь и домен не меняются.

Что и в каком порядке менять — в [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md).

## Ключевые архитектурные решения

- `IDomainEvent : INotification` + `MediatR.Contracts` в Domain — осознанное нарушение правила «домен ни от кого не зависит» ради отказа от собственного диспатчера.
- `Channel<T>` bounded 100, `FullMode.Wait` — backpressure: producer ждёт, когда очередь заполнена, вместо неограниченного роста памяти.
- `IPublisher` в воркере, а не `IMediator` — воркеру нужен только `Publish`, лишние зависимости не нужны.
- `EnsureCreated` вместо миграций — учебный проект; схема создаётся при старте.
- Demo delay 3 сек (`// demo delay, remove later`) — искусственная задержка, чтобы показать асинхронность.
- `OrderStatus` в JSON числом — фронт шлёт число, enum сериализуется как 0..3.

Подробнее эти и смежные решения разбираются в [DECISIONS.md](DECISIONS.md).

## Что дальше

- [KNOWN_SIMPLIFICATIONS.md](../KNOWN_SIMPLIFICATIONS.md) — что осознанно упрощено и чем это грозит в проде.
- [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md) — куда и в каком порядке расти.
- [EVENT_DRIVEN_DECISIONS.md](EVENT_DRIVEN_DECISIONS.md) — когда применять event-driven подход (если файл будет добавлен).
