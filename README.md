# GameServer-v3

Монорепозиторий игровой платформы v3: бекенд (AuthService, ProfileService,
RealtimeService), фронтенд, nginx и Docker. Каркас на базе GameServer-v2:
единый вход (OIDC), профиль игрока и realtime-канал; доменные сервисы
добавляются поверх.

## Backend-сервисы

| Сервис            | Описание                                                              |
| ----------------- | --------------------------------------------------------------------- |
| `AuthService`     | OIDC-провайдер (OpenIddict + ASP.NET Identity) и JSON API аккаунтов   |
| `ProfileService`  | Игровой профиль игрока (ник, регион) и identity-поля из access-токена |
| `RealtimeService` | WebSocket/SignalR push для клиентов: `liveops` и персональные уведомления |

AuthService выдаёт токены (Authorization Code + PKCE, refresh и
client_credentials) и отдаёт админские данные:

- `/connect/*`, `/.well-known/*` — OIDC endpoints;
- `/auth/register`, `/auth/login` — регистрация и вход (Identity-cookie);
- `/admin/users`, `/admin/stats` — админ-API (permission `users.view`).

При регистрации AuthService публикует `AccountRegisteredEvent` в RabbitMQ
(`Shared.IntegrationEvents.Identity`), ProfileService по нему создаёт профиль.

ProfileService отдаёт профиль текущего игрока по Bearer-токену:

- `/profiles/me` — объединённый профиль: игровые поля (ник, регион) плюс
  `email`/`userName` из claims access-токена.

RealtimeService доставляет клиентам live-уведомления через SignalR (WebSocket) с
Redis backplane:

- `POST /realtime/ticket` — одноразовый тикет для WebSocket (Bearer-токен);
- `/realtime` — SignalR hub: после подключения клиент вызывает `Authenticate(ticket)`
  и автоматически попадает в `liveops` и `player:{accountId}`.

Gateway ничего не хранит: он потребляет RabbitMQ-события и пересылает их в
группы как `RealtimeNotification`. Подписок на доменные события пока нет —
обработчики добавляются в `RealtimeService.Core/Messaging/Handlers` вместе с
очередью в `RabbitMqConfiguration`.

Порты разработки: AuthService — `5280`, RealtimeService — `5287`
(`dotnet run --project backend/<Service>/src/<Service>.Web`).

## Состав

```
GameServer-v3/
├── backend/
│   ├── Directory.Build.props        # общие настройки сборки (net10.0, анализаторы)
│   ├── Directory.Packages.props     # центральные версии пакетов
│   ├── nuget.config                 # nuget.org + приватный feed Owlip.*
│   ├── Shared/IntegrationEvents/    # интеграционные события (Identity, Realtime)
│   ├── ProfileService/              # та же раскладка Domain/Contracts/Core/Infrastructure/Web
│   ├── RealtimeService/             # Contracts/Core/Infrastructure.Redis/Web
│   └── AuthService/
│       ├── AuthService.slnx
│       ├── Dockerfile               # стадии base / build / migrations / final
│       ├── src/
│       │   ├── AuthService.Domain/                    # Account, роли
│       │   ├── AuthService.Contracts/                 # DTO запросов/ответов
│       │   ├── AuthService.Core/                      # handlers, validators, permissions
│       │   ├── AuthService.Infrastructure.Postgres/   # EF Core + Identity, миграции
│       │   └── AuthService.Web/                       # Program, endpoints, OIDC, конфиги
│       └── tests/
│           ├── AuthService.UnitTests/                 # домен, валидаторы
│           └── AuthService.IntegrationTests/          # Testcontainers + OIDC-флоу
├── frontend/                        # Next.js 16 (App Router), React 19, Tailwind v4
├── nginx.conf                       # 6100 — SPA, 6101 — API (единый origin)
├── nginx/certificates/              # self-signed сертификаты для localhost
├── scripts/
│   └── prepare-nginx-https.*        # перегенерация сертификатов
├── docker-compose.yml
└── .env.example
```

## Стек

| Слой     | Технологии                                                                                       |
| -------- | ------------------------------------------------------------------------------------------------ |
| Backend  | .NET 10, Minimal API, CQRS-handlers (`Shared.Core`), FluentValidation, EF Core + PostgreSQL, Serilog, Scalar |
| Auth     | OpenIddict (OIDC), ASP.NET Identity, JWT-валидация, permissions по ролям                          |
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS v4, TanStack Query, react-hook-form + zod, axios, zustand |
| Infra    | Docker Compose, nginx с HTTPS, отдельный контейнер миграций EF, RabbitMQ (Wolverine), Redis       |
| Тесты    | xUnit, `WebApplicationFactory`, Testcontainers.PostgreSql, Respawn                                |

## Быстрый старт (Docker)

```bash
cp .env.example .env
# заполни NUGET_USERNAME / NUGET_PASSWORD (PAT с read:packages)
docker compose up -d --build
```

- UI: https://localhost:6100
- API (OIDC + JSON): https://localhost:6101 (`/health`, в Docker — Scalar `/scalar/v1`)
- PostgreSQL: `localhost:5437`
- RabbitMQ management: http://localhost:15672 (guest/guest)

Перед первым запуском сгенерируй сертификаты для nginx:
`scripts/prepare-nginx-https.sh` (или `.ps1` на Windows). Контейнеры
`gameserver-v3-auth-migrator` и `gameserver-v3-profile-migrator` накатывают
EF-миграции до старта сервисов.

## Аутентификация и роли

Фронтенд ходит только на один origin (`https://localhost:6101`): и OIDC, и
JSON API идут через nginx. Поток входа:

1. `/login` — email и пароль уходят в `POST /auth/login`, AuthService ставит
   Identity-cookie;
2. браузер редиректится на `/connect/authorize` (Authorization Code + PKCE),
   AuthService возвращает код на `/auth/callback`;
3. SPA меняет код на токены в `/connect/token`, кладёт их в zustand-store и
   подставляет access-токен как `Bearer` через интерсептор axios;
4. `/profiles/me` отдаёт профиль игрока (ProfileService), `/admin/*` доступны по
   permission `users.view` (UI админки в v3 пока нет).

Роли (`AuthService.Domain.AuthRoles`) и permissions:

| Роль        | Permissions                              | UI                                     |
| ----------- | ---------------------------------------- | -------------------------------------- |
| `User`      | —                                        | профиль                                |
| `Moderator` | `users.view`, `users.manage`, `platform.manage` | профиль (админ-API без UI)      |
| `Admin`     | все                                      | профиль (админ-API без UI)             |
| `Service`   | —                                        | service-to-service (client_credentials) |

После входа `resolveHomeRoute` уводит на `/profile`. Для будущих закрытых
разделов есть клиентский guard `RequireAuth roles={[...]}`.

Чтобы выдать роль первому администратору:

```sql
INSERT INTO auth.account_roles (account_id, role_id)
SELECT u.id, r.id FROM auth.accounts u, auth.roles r
WHERE u.email = 'admin@example.com' AND r.name = 'Admin';
```

## Конфигурация (options)

Секции appsettings, которые биндятся на options-классы:

| Секция           | Options-класс                                          | Ключи                                                                     |
| ---------------- | ------------------------------------------------------ | ------------------------------------------------------------------------- |
| `Cors`           | `Shared.Framework.Cors.CorsSettings`                   | `AllowedOrigins`, `AllowCredentials`, `AllowedHeaders`, `AllowedMethods`  |
| `Authentication` | `Shared.Authentication...BearerTokenSettings` (JWT)    | `Authority`, `Audiences`                                                  |
| `OpenIddict`     | `AuthService.Core.Configuration.OidcOptions`           | `Issuer`, `AccessTokenLifetimeMinutes`, `RefreshTokenLifetimeDays`, `Clients:*` |
| `AuthService`    | `AuthService.Web.Configurations.Options.AuthServiceOptions` | `FrontendLoginUrl` (куда вести неавторизованный `/connect/authorize`) |

`OpenIddict__Issuer` в Docker — `https://localhost:6101/` (внешний адрес API
через nginx), `AuthService__FrontendLoginUrl` — `https://localhost:6100/login`.

## Локальная разработка

Backend (Postgres поднимаем из compose):

```bash
docker compose up -d postgres
cd backend/AuthService
dotnet run --project src/AuthService.Web
# http://localhost:5280, Scalar: http://localhost:5280/scalar/v1
```

Frontend:

```bash
cd frontend
npm install
npm run dev
# http://localhost:3000, API берётся из frontend/.env.development
```

Миграции:

```bash
dotnet ef migrations add <Name> \
  --project src/AuthService.Infrastructure.Postgres \
  --startup-project src/AuthService.Web
```

Тесты (нужен запущенный Docker — Testcontainers поднимает Postgres):

```bash
cd backend/AuthService
dotnet test
```

## API

| Метод | Путь             | Permission     | Описание                                   |
| ----- | ---------------- | -------------- | ------------------------------------------ |
| POST  | `/auth/register` | —              | Регистрация (роль `User`)                  |
| POST  | `/auth/login`    | —              | Вход, ставит Identity-cookie               |
| GET   | `/profiles/me`   | аутентификация | Профиль текущего игрока (ProfileService)   |
| GET   | `/admin/users`   | `users.view`   | Список аккаунтов с поиском и пагинацией    |
| GET   | `/admin/stats`   | `users.view`   | Сводка: всего/админы/модераторы/заблокированы |
| POST  | `/realtime/ticket` | аутентификация | Одноразовый тикет для SignalR/WebSocket |
| GET   | `/health`        | —              | Health-check (Postgres)                    |

Все ответы обёрнуты в общий `Shared.SharedKernel.Envelope`.

## Организация эндпоинтов

Эндпоинты группируются по ресурсу в класс `IEndpointGroup` (файл
`Endpoints/<Resource>/<Resource>Endpoints.cs`). Группа создаёт `MapGroup` с
общим префиксом, тегом и `RequireAuthorization()`, а конкретные маршруты
уточняют permission, имя и summary:

```csharp
public sealed class AdminEndpoints : IEndpointGroup
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        group.MapGet("/users", ListUsers)
            .WithName("Admin.Users.List")
            .RequirePermissions(AuthPermissions.Users.VIEW);

        return group;
    }
}
```

Группы регистрируются автоматически: `app.MapEndpointGroups(assembly)` в
`AppConfigurationExtensions` находит все `IEndpointGroup`. Handler'ы
разрешаются из DI по интерфейсам `ICommandHandler<,>` / `IQueryHandler<,>`
(регистрируются `AddCore`/`AddHandlers`).

## Новый доменный сервис

1. Создай проект по образцу `backend/AuthService` (Domain / Contracts / Core /
   Infrastructure.Postgres / Web) и добавь сервис и migrator в
   `docker-compose.yml`;
2. создай отдельную БД через `POSTGRES_DB`/init-скрипт и свою схему;
3. добавь nginx `location` с префиксом домена (как `/profiles`) на
   сервере `6101` и отдельный `upstream`;
4. зарегистрируй scope/resource в `OidcScopes`/`PlatformSyncService` AuthService
   и укажи audience нового сервиса в `Authentication:Audiences`.

## Соглашения

- Каждая фича — папка `Features/<Domain>/{Commands|Queries}/<UseCase>` с
  `Command/Query`, `Handler`, `Validator`.
- Эндпоинты — один `IEndpointGroup` на ресурс: общие префикс/теги/авторизация
  на группе, `.WithName`/`.WithSummary`/`RequirePermissions` — на маршрутах.
- Handler возвращает `Result<T, Error>` из CSharpFunctionalExtensions; ошибки
  конвертируются в HTTP-статусы middleware из `Shared.Framework`.
- Данные читаются через `IReadDbContext` (no-tracking), пишутся через
  репозиторий/менеджеры Identity.
- Валидация — FluentValidation + `GeneralErrors` из `Shared.SharedKernel`.
- Схема БД — своя (`auth`), история миграций в `__EFMigrationsHistory`.
- Фронтенд: `entities` (API/DTO), `features` (model/ui), `shared` (auth, api,
  ui); роли и guard'ы — в `shared/auth`.
