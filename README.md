# OrderEvents_Lite

Учебный проект по Domain Events и event-driven подходу in-process: ASP.NET Core 10, CQRS через MediatR и асинхронная обработка доменных событий без микросервисов.

## Цель проекта

Разобрать механику доменных событий на практике:

- как агрегат накапливает события и кто их публикует;
- чем синхронный dispatch отличается от асинхронной очереди;
- зачем нужен `BackgroundService` и отдельный DI-scope на обработку;
- почему вызов внешнего API (например, Telegram) не делают внутри транзакции.

Осознанные упрощения и их последствия в проде вынесены в [KNOWN_SIMPLIFICATIONS.md](KNOWN_SIMPLIFICATIONS.md).

## Технологии

- ASP.NET Core 10
- Clean Architecture (5 проектов)
- CQRS через MediatR
- Domain Events: `IDomainEvent : INotification`, обработка через MediatR (`INotificationHandler<T>`)
- EF Core + SQLite
- `System.Threading.Channels` (in-process очередь)
- `BackgroundService`
- Vue 3 + Vite + TypeScript (фронт-шаблон)

## Структура решения

```text
OrderEvents_Lite.slnx
├── src/Backend/OrderEvents_Lite/
│   ├── PS.OrderEvents.API            Web API, контроллеры, DI
│   ├── PS.OrderEvents.Application    CQRS, абстракции, хендлеры событий
│   ├── PS.OrderEvents.Common         пусто, зарезервировано под логирование
│   ├── PS.OrderEvents.Domain         Order, IDomainEvent, события, OrderStatus
│   └── PS.OrderEvents.Infrastructure EF Core, SQLite, очередь, воркер, Telegram-заглушка
└── src/Frontend/PS.OrderEvents       Vue 3 + Vite + TS, пока шаблон, без страниц заказов
```

Правило зависимостей:

- `API` → `Application`, `Infrastructure`
- `Application` → `Domain`
- `Infrastructure` → `Application`
- `Domain` не зависит ни от кого, кроме `MediatR.Contracts` (только интерфейсы, без транзитивных зависимостей)

## Пайплайн обработки

```text
POST /orders
  → OrdersController.Create
  → mediator.Send(CreateOrderCommand)
  → CreateOrderCommandHandler
  → Order.Create()                    ← здесь поднимается OrderCreated
  → repository.AddAsync
  → unitOfWork.SaveChangesAsync
      → context.SaveChangesAsync      ← заказ в БД (коммит)
      → собрали DomainEvents из ChangeTracker
      → foreach → queue.EnqueueAsync  ← положили в Channel, НЕ ждём
      → ClearDomainEvents
  → 201 Created (быстро, ~300 мс)

[фон, DomainEventQueueProcessor]
  → читает Channel
  → Task.Delay(3000)                  ← demo delay, remove later
  → scope → IPublisher.Publish
      → INotificationHandler<OrderCreated>  → [EVENT] Order created
      → TelegramOrderCreatedHandler         → [TELEGRAM] Would send notification
  → ошибки ловятся, воркер живёт
```

## Как запустить

### Бэкенд

```text
cd src/Backend/OrderEvents_Lite
dotnet run --project PS.OrderEvents.API
```

API доступен на `http://localhost:5275` и `https://localhost:7093`. База `orderevents.db` создаётся автоматически при старте (`EnsureCreated`).

### Фронтенд

```text
cd src/Frontend/PS.OrderEvents
npm install
npm run dev
```

Фронт пока остаётся шаблоном, API-клиент не подключён — известный разрыв.

## Эндпоинты

| Метод | Путь | Тело | Ответ |
|-------|------|------|-------|
| `POST` | `/orders` | — | `201 Created`, `Location: /orders/{id}`, тело — GUID |
| `PATCH` | `/orders/{id}/status` | `{ "newStatus": 1 }` | `204 No Content` |
| `GET` | `/orders/{id}` | — | `200` / `404` |
| `GET` | `/orders` | — | `200`, массив |

`OrderStatus` в JSON передаётся числом: `0 = Pending`, `1 = Paid`, `2 = Shipped`, `3 = Completed`.

## Как проверить цепочку

1. Запустить API.
2. Выполнить `POST /orders` (curl, Postman или `.http`-файл) — ответ `201` приходит быстро.
3. Через ~3 секунды в консоли API появляются:
   - `[EVENT] Order created: <id>` — `OrderCreatedHandler`;
   - `[TELEGRAM] Would send notification for order <id>` — `TelegramOrderCreatedHandler`.
4. Ответ мгновенный, обработка — в фоне. Это и есть event-driven: запрос не ждёт побочных эффектов.

Порядок строк между хендлерами не гарантирован.

## Симуляция сбоя Telegram

В `appsettings.json` установить:

```json
"Telegram": {
  "SimulateFailure": true
}
```

Перезапустить API и выполнить `POST /orders`. В консоли появится `[TELEGRAM ERROR] ...`, но `[EVENT]` от `OrderCreatedHandler` будет, заказ останется в БД, а воркер продолжит обрабатывать следующие события.

## Что дальше / чего здесь нет

Проект намеренно не содержит продакшн-механизмов: Outbox, retry/dead-letter, идемпотентности, внешней шины (RabbitMQ/Kafka), реального Telegram Bot API, auth и тестов. Полный список с объяснением рисков — в [KNOWN_SIMPLIFICATIONS.md](KNOWN_SIMPLIFICATIONS.md). (MediatR 14 без license key логирует warning — для учебного проекта допустимо.)
