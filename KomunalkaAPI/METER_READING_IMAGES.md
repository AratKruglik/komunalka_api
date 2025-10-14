# Meter Reading Images Feature

## Огляд

Функціонал додавання фотографій лічильників до показань з автоматичною оптимізацією зображень у фоновому режимі.

## Особливості

- ✅ Завантаження фото при створенні показання лічильника
- ✅ Автоматична оптимізація зображень (без збереження оригіналу)
- ✅ Створення thumbnail (200px) та оптимізованої версії (800px)
- ✅ Фонова обробка через `ImageProcessingService`
- ✅ Валідація файлів за розміром, форматом та сигнатурою
- ✅ Видалення EXIF-метаданих для приватності
- ✅ Автоматичне обертання за EXIF-орієнтацією

## API Endpoints

### 1. Створення показання з фото

```http
POST /api/v1/meterreading
Content-Type: multipart/form-data
Authorization: Bearer {token}

Form data:
- ServiceCounterId: integer (required)
- Value: float (required)
- image: file (optional, max 10MB, JPEG/PNG/WebP)
```

**Response 201 Created:**
```json
{
  "data": {
    "id": 1,
    "serviceCounterId": 1,
    "value": 150.75,
    "createdAt": "2025-10-14T12:00:00Z",
    "updatedAt": "2025-10-14T12:00:00Z",
    "images": []
  }
}
```

### 2. Отримання показання з фото

```http
GET /api/v1/meterreading/{id}
Authorization: Bearer {token}
```

**Response 200 OK:**
```json
{
  "data": {
    "id": 1,
    "serviceCounterId": 1,
    "value": 150.75,
    "createdAt": "2025-10-14T12:00:00Z",
    "updatedAt": "2025-10-14T12:00:00Z",
    "images": [
      {
        "id": 1,
        "serviceCounterValueId": 1,
        "optimizedUrl": "/api/v1/meterreading/images/1/optimized",
        "thumbnailUrl": "/api/v1/meterreading/images/1/thumbnail",
        "optimizedSizeInBytes": 45678,
        "thumbnailSizeInBytes": 12345,
        "width": 1920,
        "height": 1080,
        "mimeType": "image/jpeg",
        "isProcessed": true,
        "createdAt": "2025-10-14T12:00:00Z"
      }
    ]
  }
}
```

### 3. Отримання оптимізованого зображення

```http
GET /api/v1/meterreading/images/{id}/optimized
```

**Response:** Image file (JPEG)

### 4. Отримання thumbnail

```http
GET /api/v1/meterreading/images/{id}/thumbnail
```

**Response:** Image file (JPEG)

### 5. Видалення показання

```http
DELETE /api/v1/meterreading/{id}
Authorization: Bearer {token}
```

**Response:** 204 No Content

## Конфігурація

### appsettings.json

```json
{
  "ImageSettings": {
    "StoragePath": "wwwroot/uploads/meter-readings",
    "MaxFileSizeInMB": 10,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"],
    "OptimizedImageWidth": 800,
    "ThumbnailWidth": 200,
    "JpegQuality": 85
  }
}
```

## Структура файлів

```
wwwroot/
  uploads/
    meter-readings/
      {year}/
        {month}/
          {userId}/
            {readingId}_{timestamp}_optimized.jpg
            {readingId}_{timestamp}_thumbnail.jpg
```

## База даних

### Таблиця: meter_reading_images

| Поле | Тип | Опис |
|------|-----|------|
| id | integer | Primary key |
| service_counter_value_id | integer | Foreign key до service_counter_values |
| optimized_path | varchar(500) | Шлях до оптимізованого зображення |
| thumbnail_path | varchar(500) | Шлях до thumbnail |
| optimized_size_in_bytes | bigint | Розмір оптимізованого файлу |
| thumbnail_size_in_bytes | bigint | Розмір thumbnail |
| width | integer | Ширина оригінального зображення |
| height | integer | Висота оригінального зображення |
| mime_type | varchar(50) | MIME type |
| is_processed | boolean | Чи оброблено зображення |
| created_at | timestamptz | Дата створення |
| updated_at | timestamptz | Дата оновлення |

## Workflow

1. Користувач завантажує показання з фото через `POST /meterreading`
2. API зберігає файл у тимчасову директорію
3. Створюється запис у БД з `is_processed = false`
4. Завдання додається в чергу `ImageProcessingService`
5. Фоновий сервіс обробляє зображення:
   - Завантажує з temp файлу
   - Автоматично обертає за EXIF
   - Видаляє EXIF-метадані
   - Створює оптимізовану версію (800px, JPEG 85%)
   - Створює thumbnail (200px, JPEG 85%)
   - Зберігає у файлову систему
   - Оновлює запис у БД: `is_processed = true`
   - Видаляє temp файл

## Обмеження та валідація

- Максимальний розмір файлу: **10 MB**
- Дозволені формати: **JPEG, PNG, WebP**
- Перевірка сигнатури файлу (захист від підміни розширення)
- Мінімальна роздільність: немає обмеження

## Приклади використання

### cURL

```bash
# Створити показання з фото
curl -X POST http://localhost:8080/api/v1/meterreading \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -F "ServiceCounterId=1" \
  -F "Value=150.75" \
  -F "image=@/path/to/meter-photo.jpg"

# Отримати показання
curl -X GET http://localhost:8080/api/v1/meterreading/1 \
  -H "Authorization: Bearer YOUR_TOKEN"

# Завантажити оптимізоване зображення
curl -X GET http://localhost:8080/api/v1/meterreading/images/1/optimized \
  -o meter-optimized.jpg
```

## Troubleshooting

### Зображення не обробляється

1. Перевірте логи: `docker-compose logs -f api`
2. Переконайтесь, що `ImageProcessingService` запущений
3. Перевірте наявність `wwwroot/uploads/meter-readings/` директорії
4. Перевірте права доступу до файлової системи

### Помилка "File too large"

Збільште `MaxFileSizeInMB` в `appsettings.json` та перезапустіть API.

### Помилка "Invalid image format"

Переконайтесь, що файл дійсно є зображенням у форматі JPEG/PNG/WebP.

## Майбутні покращення

- [ ] Додати підтримку WebP для оптимізованих зображень
- [ ] Додати можливість завантаження кількох фото до одного показання
- [ ] Додати OCR для автоматичного розпізнавання показань
- [ ] Додати інтеграцію з хмарним сховищем (AWS S3, Azure Blob)
- [ ] Додати watermark до зображень
