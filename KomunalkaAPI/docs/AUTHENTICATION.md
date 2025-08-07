# Аутентифікація в Komunalka API

Komunalka API використовує JWT (JSON Web Tokens) для аутентифікації та авторизації користувачів. Ця документація описує механізм аутентифікації, основні принципи та ендпоінти.

## Механізм аутентифікації

### JWT (JSON Web Tokens)

JWT - це відкритий стандарт для створення токенів доступу, що містять зашифровану інформацію про користувача. Кожен JWT токен складається з трьох частин:

1. **Заголовок (Header)** - містить тип токена та алгоритм шифрування
2. **Корисне навантаження (Payload)** - містить дані про користувача (claims)
3. **Підпис (Signature)** - забезпечує цілісність токена

### Схема аутентифікації

API використовує схему Bearer для аутентифікації:

```
Authorization: Bearer {jwt_token}
```

### Токени оновлення (Refresh Tokens)

Для забезпечення тривалого доступу без постійного повторного входу використовуються токени оновлення. Основні характеристики:

- Довший термін дії (7 днів за замовчуванням)
- Зберігаються в базі даних
- Одноразового використання (не можуть бути використані повторно)
- Можуть бути відкликані

## Ендпоінти аутентифікації

### Реєстрація користувача

```
POST /api/auth/register
```

**Тіло запиту:**

```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "confirmPassword": "string"
}
```

**Відповідь:**

```json
{
  "userId": 0,
  "username": "string",
  "email": "string",
  "role": "string",
  "token": "string",
  "refreshToken": "string",
  "expiration": "2023-09-01T12:00:00Z"
}
```

### Вхід користувача

```
POST /api/auth/login
```

**Тіло запиту:**

```json
{
  "email": "string",
  "password": "string"
}
```

**Відповідь:**

```json
{
  "userId": 0,
  "username": "string",
  "email": "string",
  "role": "string",
  "token": "string",
  "refreshToken": "string",
  "expiration": "2023-09-01T12:00:00Z"
}
```

### Оновлення токена

```
POST /api/auth/refresh-token
```

**Тіло запиту:**

```json
{
  "refreshToken": "string"
}
```

**Відповідь:**

```json
{
  "userId": 0,
  "username": "string",
  "email": "string",
  "role": "string",
  "token": "string",
  "refreshToken": "string",
  "expiration": "2023-09-01T12:00:00Z"
}
```

### Відкликання токена

```
POST /api/auth/revoke-token
```

**Тіло запиту:**

```json
{
  "refreshToken": "string"
}
```

**Відповідь:**

```json
{
  "message": "Токен успішно відкликано"
}
```

### Перевірка токена

```
GET /api/auth/validate-token
```

**Заголовки:**

```
Authorization: Bearer {jwt_token}
```

**Відповідь:**

```json
{
  "message": "Токен дійсний"
}
```

## Захист ендпоінтів

Для захисту ендпоінтів використовуйте атрибут `[Authorize]`:

```csharp
[Authorize]
[HttpGet("protected-endpoint")]
public IActionResult ProtectedEndpoint()
{
    return Ok(new { Message = "Це захищений ендпоінт" });
}
```

Для обмеження доступу за ролями використовуйте атрибут `[Authorize(Roles = "Admin")]`:

```csharp
[Authorize(Roles = "Admin")]
[HttpGet("admin-endpoint")]
public IActionResult AdminEndpoint()
{
    return Ok(new { Message = "Це ендпоінт для адміністраторів" });
}
```

## Отримання інформації про поточного користувача

Для отримання інформації про аутентифікованого користувача в контролері:

```csharp
[Authorize]
[HttpGet("current-user")]
public IActionResult GetCurrentUser()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = User.FindFirst(ClaimTypes.Name)?.Value;
    var email = User.FindFirst(ClaimTypes.Email)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;

    return Ok(new { UserId = userId, Username = username, Email = email, Role = role });
}
```

## Безпека

### Зберігання JWT на клієнті

- Зберігайте JWT токен у `localStorage` або `sessionStorage` для веб-додатків
- Використовуйте HttpOnly cookies для більшої безпеки (потребує додаткової конфігурації на сервері)
- Токени оновлення повинні зберігатися в безпечному місці (HttpOnly cookies)

### Захист від вразливостей

- Перевірка походження (Origin) запитів
- Використання HTTPS
- Короткий термін дії JWT токенів (30 хвилин за замовчуванням)
- Можливість відкликання токенів оновлення

## Поради з інтеграції

### Додавання заголовка авторизації

Для JavaScript:

```javascript
const token = localStorage.getItem('jwtToken');

fetch('https://api.example.com/protected-endpoint', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

### Обробка закінчення терміну дії токена

1. Перехоплюйте 401 відповіді
2. Використовуйте токен оновлення для отримання нового JWT
3. Повторіть оригінальний запит з новим токеном

```javascript
async function fetchWithTokenRefresh(url, options) {
  try {
    const response = await fetch(url, {
      ...options,
      headers: {
        ...options.headers,
        'Authorization': `Bearer ${localStorage.getItem('jwtToken')}`
      }
    });

    if (response.status === 401) {
      // Токен закінчився, оновлюємо
      const refreshToken = localStorage.getItem('refreshToken');
      const refreshResponse = await fetch('/api/auth/refresh-token', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken })
      });

      if (refreshResponse.ok) {
        const refreshData = await refreshResponse.json();
        localStorage.setItem('jwtToken', refreshData.token);
        localStorage.setItem('refreshToken', refreshData.refreshToken);

        // Повторюємо оригінальний запит
        return fetch(url, {
          ...options,
          headers: {
            ...options.headers,
            'Authorization': `Bearer ${refreshData.token}`
          }
        });
      } else {
        // Не вдалося оновити токен, перенаправляємо на сторінку входу
        window.location.href = '/login';
      }
    }

    return response;
  } catch (error) {
    console.error('Помилка запиту:', error);
    throw error;
  }
}
```

## Висновок

Правильна реалізація JWT аутентифікації забезпечує надійний захист вашого API. Використовуйте цю документацію як керівництво при інтеграції аутентифікації у вашому клієнтському додатку.
