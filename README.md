# Выполнил Кочуров Сергей
# 
# Severstal Warehouse API

Backend для работы со складом рулонов металла. Проект написан на ASP.NET Core Web API, данные хранятся в PostgreSQL.

## Возможности

- Добавление рулона на склад.
- Удаление рулона со склада по `id`.
- Получение рулонов с фильтрацией по диапазонам.
- Получение статистики за период.
- Swagger UI для ручной проверки API.
- Docker Compose для запуска API вместе с PostgreSQL.
- Unit-тесты без обращения к реальной базе данных.

## Запуск через Docker

Из корня проекта:

```powershell
docker compose up --build
```

После запуска:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`

Остановить контейнеры:

```powershell
docker compose down
```

Остановить контейнеры и удалить данные PostgreSQL:

```powershell
docker compose down -v
```

## Конфигурация БД

Строка подключения читается из `ConnectionStrings:WarehouseDb`.

В `appsettings.json` задано значение по умолчанию:

```json
"ConnectionStrings": {
  "WarehouseDb": "Host=localhost;Port=5432;Database=severstal_warehouse;Username=postgres;Password=postgres"
}
```

В Docker Compose строка подключения переопределяется через ENV:

```yaml
ConnectionStrings__WarehouseDb: Host=postgres;Port=5432;Database=severstal_warehouse;Username=postgres;Password=postgres
```

Для локального запуска можно задать переменную окружения:

```powershell
$env:ConnectionStrings__WarehouseDb="Host=localhost;Port=5432;Database=severstal_warehouse;Username=postgres;Password=postgres"
```

## Основные endpoint'ы

Добавить рулон:

```http
POST /api/coils
Content-Type: application/json

{
  "length": 1,
  "weight": 2
}
```

Получить все рулоны:

```http
GET /api/coils
```

Получить рулон по id:

```http
GET /api/coils/{id}
```

Удалить рулон со склада:

```http
DELETE /api/coils/{id}
```

Получить только рулоны, которые сейчас на складе:

```http
GET /api/coils?onlyInStock=true
```

Пример фильтрации по нескольким диапазонам:

```http
GET /api/coils?idFrom=1&idTo=10&weightFrom=100&weightTo=500&lengthFrom=1&lengthTo=100
```

Получить статистику за период:

```http
GET /api/coils/statistics?from=2026-01-01T00:00:00Z&to=2026-01-31T23:59:59Z
```

## Запуск без Docker

Нужен запущенный PostgreSQL и корректная строка подключения.

```powershell
dotnet run --project SeverstalWarehouse.Api
```

Адреса для локального запуска берутся из `SeverstalWarehouse.Api/Properties/launchSettings.json`.

Обычно Swagger доступен здесь:

```text
http://localhost:5135/swagger
```

## Тесты

Запустить все тесты:

```powershell
dotnet test
```

Тесты находятся в проекте `SeverstalWarehouse.Tests`.

Они проверяют бизнес-логику `CoilService` и используют ручной mock `MockCoilRepository`, поэтому не подключаются к PostgreSQL.
