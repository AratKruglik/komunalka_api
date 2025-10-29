# Local Development Guide

> **⚠️ SECURITY NOTE**: This documentation contains example credentials and connection strings for LOCAL DEVELOPMENT ONLY. Never use these values in production or commit real secrets to git.

## Робота з міграціями EF Core

### Проблема

При запуску `dotnet ef database update` локально, EF Core намагається підключитися до `db:5432` (Docker hostname), але база даних доступна тільки через `localhost:5432`.

### Рішення

#### Варіант 1: Використання явного connection string (рекомендовано)

```bash
# Example connection string for LOCAL development only
dotnet ef database update --connection "Host=localhost;Port=5432;Database=komunalka;Username=postgres;Password=postgres"
```

#### Варіант 2: Створення .env.local

Створіть файл `.env.local` з налаштуваннями для локальної розробки:

```env
# Example values for LOCAL development only - DO NOT commit real secrets!
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DATABASE=komunalka
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=postgres
```

Потім завантажте змінні:

```bash
export $(cat .env.local | xargs) && dotnet ef database update
```

#### Варіант 3: Тимчасова зміна .env

Тимчасово змініть `POSTGRES_HOST=db` на `POSTGRES_HOST=localhost` в `.env`, запустіть міграцію, потім поверніть назад.

**ВАЖЛИВО:** Не комітьте .env з localhost в git!

### Створення нової міграції

```bash
# Створити міграцію
dotnet ef migrations add YourMigrationName

# Застосувати міграцію (з явним connection string)
dotnet ef database update --connection "Host=localhost;Port=5432;Database=komunalka;Username=postgres;Password=postgres"
```

### Видалення міграції

```bash
# Видалити останню міграцію (якщо не застосована)
dotnet ef migrations remove

# Відкотити до попередньої міграції
dotnet ef database update PreviousMigrationName --connection "Host=localhost;Port=5432;Database=komunalka;Username=postgres;Password=postgres"
```

## Робота з Docker

### Запуск проєкту

```bash
# Запустити всі сервіси
docker-compose up -d

# Переглянути логи
docker-compose logs -f api

# Перезібрати та перезапустити API
docker-compose up -d --build api

# Зупинити всі сервіси
docker-compose down
```

### Перевірка стану

```bash
# Статус контейнерів
docker-compose ps

# Логи конкретного сервісу
docker-compose logs -f api
docker-compose logs -f db

# Підключення до PostgreSQL
docker exec -it komunalka-db psql -U postgres -d komunalka
```

### Корисні команди SQL

```sql
-- Переглянути всі таблиці
\dt

-- Структура таблиці
\d meter_reading_images

-- Переглянути дані
SELECT * FROM meter_reading_images;

-- Переглянути застосовані міграції
SELECT * FROM "__EFMigrationsHistory";
```

## Локальна розробка без Docker

### 1. Запустіть PostgreSQL локально

```bash
# Або використовуйте Docker тільки для БД
docker run -d \
  --name postgres-local \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=komunalka \
  -p 5432:5432 \
  postgres:17-alpine
```

### 2. Налаштуйте .env

Змініть `POSTGRES_HOST=localhost` в `.env`

### 3. Запустіть API

```bash
dotnet run
```

API буде доступне на:
- HTTP: http://localhost:5242
- HTTPS: https://localhost:7095
- Swagger: http://localhost:5242/swagger

## Тестування Meter Reading Images

### 1. Отримайте JWT токен

```bash
# Example test credentials for LOCAL development only
# Зареєструйте користувача
curl -X POST http://localhost:8080/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Password123",
    "confirmPassword": "Password123"
  }'

# Увійдіть
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Password123"
  }'
```

### 2. Створіть показання з фото

```bash
# Replace YOUR_JWT_TOKEN with actual token from login response
curl -X POST http://localhost:8080/api/v1/meterreading \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "ServiceCounterId=1" \
  -F "Value=150.75" \
  -F "image=@/path/to/your/meter-photo.jpg"
```

### 3. Перегляньте результат

```bash
# Replace YOUR_JWT_TOKEN with actual token from login response
# Отримати показання з метаданими фото
curl -X GET http://localhost:8080/api/v1/meterreading/1 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Завантажити оптимізоване фото
curl -X GET http://localhost:8080/api/v1/meterreading/images/1/optimized \
  -o meter-optimized.jpg

# Завантажити thumbnail
curl -X GET http://localhost:8080/api/v1/meterreading/images/1/thumbnail \
  -o meter-thumbnail.jpg
```

## Відлагодження

### Увімкнути детальні логи

В `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Перевірити фонову обробку зображень

```bash
# Переглянути логи ImageProcessingService
docker-compose logs -f api | grep "Image Processing"

# Перевірити чи створені файли
ls -la wwwroot/uploads/meter-readings/
```

### Очистити базу даних

```bash
# Видалити всі дані
docker exec -it komunalka-db psql -U postgres -d komunalka -c "TRUNCATE TABLE meter_reading_images, service_counter_values CASCADE;"

# Або повністю перестворити базу
docker-compose down -v
docker-compose up -d
```

## Productio тіпси

1. **Оновіть EF Core Tools:**
   ```bash
   dotnet tool update --global dotnet-ef
   ```

2. **Використовуйте proper secrets management:**
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault

3. **Налаштуйте backup для uploads:**
   - Регулярне резервне копіювання `wwwroot/uploads`
   - Або використовуйте cloud storage (S3, Azure Blob)

4. **Моніторинг:**
   - Перевіряйте розмір папки uploads
   - Моніторьте фоновий сервіс ImageProcessingService
   - Налаштуйте alerts для помилок обробки зображень

## Корисні посилання

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SixLabors.ImageSharp](https://docs.sixlabors.com/articles/imagesharp/index.html)
- [ASP.NET Core Background Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
