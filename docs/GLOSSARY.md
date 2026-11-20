# Словарь терминов

Короткий справочник по терминам, которые встречаются в проекте и документации. Не учебник и не замена основным документам: как устроено — в [ARCHITECTURE.md](ARCHITECTURE.md), почему так — в [DECISIONS.md](DECISIONS.md), когда применять event-driven — в [EVENT_DRIVEN_DECISIONS.md](EVENT_DRIVEN_DECISIONS.md), что упрощено — в [KNOWN_SIMPLIFICATIONS.md](../KNOWN_SIMPLIFICATIONS.md), куда расти — в [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md).

## События и сообщения

- **Domain event** — факт, произошедший в домене (`OrderCreated`, `OrderStatusChanged`). В проекте — `IDomainEvent : INotification`. Прошлое время, не команда.
- **Integration event** — событие, пересекающее границу сервиса, контракт между системами (внешняя шина). В проекте не используется, см. [EVENT_DRIVEN_DECISIONS.md](EVENT_DRIVEN_DECISIONS.md).
- **Command** — намерение что-то сделать (`CreateOrderCommand`). Повелительное, один обработчик, может быть отклонена.
- **Query** — запрос данных без побочных эффектов (`GetOrderQuery`). Один обработчик, возвращает DTO.
- **Notification** — механизм MediatR для pub/sub (`INotification`, `INotificationHandler<T>`). В проекте — доменные события.

## CQRS и MediatR

- **CQRS** — разделение команд (запись) и запросов (чтение). В проекте — через MediatR, без раздельных БД.
- **MediatR** — библиотека in-process медиатора: `IRequest`/`IRequestHandler` (один обработчик), `INotification`/`INotificationHandler` (много). Версия 14.
- **IRequest / IRequestHandler** — команда/запрос и её обработчик (один к одному).
- **INotification / INotificationHandler\<T\>** — событие и его обработчики (один ко многим).
- **IPublisher** — публикация уведомления (`Publish`). В проекте — в `DomainEventQueueProcessor`.
- **IMediator** — полный API медиатора (`Send`, `Publish`). В проекте не используется в воркере (только `IPublisher`).
- **MediatR.Contracts** — пакет только с интерфейсами (`INotification` и др.), без транзитивных зависимостей. В проекте — в Domain.

## Очередь и обработка

- **In-process очередь** — очередь в памяти процесса (`Channel<T>`). Теряется при рестарте. В проекте — `InMemoryDomainEventQueue`.
- **Channel\<T\>** — примитив .NET для producer/consumer. В проекте — bounded 100, `FullMode.Wait`.
- **Backpressure** — механизм «притормозить producer, если consumer не успевает». В проекте — `FullMode.Wait`.
- **BackgroundService** — hosted-сервис для фоновой работы. В проекте — `DomainEventQueueProcessor`.
- **IServiceScopeFactory** — фабрика DI-scope. В проекте — воркер создаёт scope на событие (singleton не может инжектить scoped).
- **Scope** — время жизни scoped-зависимостей (обычно HTTP-запрос или единица работы). В проекте — на каждое событие в воркере.

## Гарантии доставки

- **At-most-once** — «не более одного раза»: может потеряться, не дублируется. In-process Channel без Outbox — близко к этому.
- **At-least-once** — «не менее одного раза»: не теряется, но возможны дубли. Внешняя шина по умолчанию. Требует идемпотентности.
- **Exactly-once** — «ровно один раз»: на практике недостижимо в распределённой системе; эмуляция через at-least-once + идемпотентность.
- **Идемпотентность** — свойство обработчика: повторная обработка того же события не меняет результат. В проекте нет (см. [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md)).
- **Eventual consistency** — «данные согласованы не сразу, но в итоге». Цена event-driven. В проекте: событие обрабатывается через ~3 сек (demo delay).
- **Outbox Pattern** — таблица `OutboxMessages`, запись в той же транзакции, что и бизнес-изменение; фоновый publisher публикует. Решает потерю события между коммитом и публикацией. В проекте нет (roadmap).
- **Dead-letter** — «очередь мёртвых писем»: события, которые не удалось обработать после N попыток. В проекте нет (roadmap).
- **Retry** — повторная попытка обработки при сбое. В проекте нет (roadmap).

## Архитектура

- **Clean Architecture** — слои с правилом зависимостей (Domain ← Application ← Infrastructure/API). В проекте — 5 проектов.
- **Агрегат (Aggregate)** — корень согласованности (`Order`). Накапливает события.
- **Domain event в агрегате** — событие поднимается в момент изменения состояния (`Order.Create`, `ChangeStatus`), накапливается в `List<IDomainEvent>`.
- **Unit of Work** — единица работы: коммит + сбор событий + постановка в очередь. В проекте — `UnitOfWork`.
- **Шов (seam)** — точка расширения, где можно заменить реализацию без правок домена (`IDomainEventQueue`, `IUnitOfWork`, `ITelegramNotifier`).

## Прочее

- **EnsureCreated** — создание схемы EF Core без миграций. В проекте — при старте.
- **JsonStringEnumConverter** — конвертер enum в JSON строкой. В проекте не используется (enum числом).
- **Заглушка (Stub)** — фейковая реализация для демо/тестов (`TelegramNotifierStub`).

## Заключение

Если термин не найден — смотри [ARCHITECTURE.md](ARCHITECTURE.md), [DECISIONS.md](DECISIONS.md), [EVENT_DRIVEN_DECISIONS.md](EVENT_DRIVEN_DECISIONS.md) и [PRODUCTION_ROADMAP.md](PRODUCTION_ROADMAP.md). Словарь не заменяет эти документы, а помогает их читать.
