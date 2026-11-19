# KNOWN_SIMPLIFICATIONS

Осознанные упрощения учебного проекта OrderEvents_Lite. Для каждого пункта указано, чем он является и чем грозит в продакшене.

## Event dispatching / очередь

- **Нет Outbox.** `SaveChangesAsync` сначала коммитит заказ, затем события кладутся в очередь. Если процесс упадёт между коммитом и `EnqueueAsync`, событие потеряно навсегда. В проде — transactional outbox.
- **Нет retry.** Упавший хендлер не перезапустится; остаётся только лог (в Telegram-хендлере — log & swallow). В проде — retry / dead-letter.
- **Нет идемпотентности.** При переходе на внешнюю шину события начнут дублироваться; хендлеры должны быть идемпотентны.
- **Один воркер, последовательная обработка.** N событий обрабатываются за N × 3 секунды (демо-задержка). В проде — параллелизм / несколько воркеров / `SingleReader = false`.
- **Demo delay 3000 мс в `DomainEventQueueProcessor`.** Искусственная задержка для демонстрации sync vs async. Убрать в проде (помечена `// demo delay, remove later`).
- **In-process очередь (`Channel<T>`).** При рестарте необработанные события теряются. В проде — RabbitMQ/Kafka.

## Domain

- **Нет правил переходов статусов.** `ChangeStatus` разрешает любой переход (например, Pending → Completed напрямую). В проде — валидация переходов / state machine.
- **`Guid.NewGuid()` внутри `Order.Create()`.** В тестах Id непредсказуем. В проде — передавать снаружи.
- **`DateTime.UtcNow` в домене.** Нет тестируемых часов (`IClock`). В проде — абстракция времени.
- **Нет валидации полей (null/пустые), value objects нет.** В проде — валидация на границе и в домене.

## Infrastructure / API

- **`EnsureCreated()`, не миграции.** При изменении модели — удалить `orderevents.db` и пересоздать либо перейти на миграции. `Microsoft.EntityFrameworkCore.Design` лежит только в Infrastructure с `PrivateAssets=all`; для `dotnet ef` из API понадобится доработка.
- **`DomainEventDispatcher` резолвит хендлеры из внедрённого `IServiceProvider`.** Работает, пока диспатчер резолвится из scope (воркер создаёт scope — ок). Из корня/синглтона упадёт.
- **`GetServices<IEventHandler>()` жадно инстанцирует всех хендлеров** на каждый dispatch, даже неподходящих. Фильтрация по `CanHandle` — после создания. Для обучения ок.
- **`IEventHandler` регистрируется как необобщённый `IEventHandler`, не `IEventHandler<T>`.** Иначе диспатчер (`GetServices<IEventHandler>()`) их не найдёт. Scoped, не singleton (scoped `DbContext`).
- **`AddApplication()` находится в API, не в Application.** Не тащим `Microsoft.Extensions.DependencyInjection.Abstractions` в Application ради extension'а.
- **Enum `OrderStatus` в JSON — числом (0..3), не строкой.** Фронт шлёт число.
- **PATCH not-found → 404 через `catch (InvalidOperationException)` в контроллере.** В проде — Result-тип / кастомное исключение.
- **Хендлеры пишут через `Console.WriteLine`, не `ILogger`.** Следующий уровень.
- **CORS открыт для `localhost:5173` (http/https), auth нет.** В проде — ограниченный список origin и аутентификация.
- **Telegram — заглушка.** Реального HTTP нет, `SimulateFailure` для демонстрации. В проде — Bot API, токен в секретах, ретраи.

## Фронтенд

- **Vue-шаблон, страниц заказов нет, `apiClient` не подключён.** Известный разрыв.
- **`localhost-key.pem` (приватный ключ) закоммичен в репозиторий.** Известный issue безопасности, вне scope проекта. В проде — не коммитить ключи, `.env.local` в `.gitignore`.

## Прочее

- **Нет тестов** (`tests/` пуст).
- **Нет Docker/compose.**
- **`PS.OrderEvents.Common` пуст** (зарезервирован под логирование).
- **Предупреждение `CS1066` в `DomainEventHandler.cs:12`** (дефолт `ct` у явной реализации интерфейса) — предсуществующее, не блокер.
