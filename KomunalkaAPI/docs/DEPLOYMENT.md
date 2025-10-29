# Komunalka API – Інструкція з деплою

> **Коротко:** деплой відбувається через GitHub Releases. Після публікації тега `vX.Y.Z` workflow `release-docker-deploy` збирає Docker-імедж, пушить його у GHCR і оновлює стек на self-hosted runner’і через `docker compose`.

## 1. Попередні вимоги

1. **Self-hosted runner**
   - Потрібен Linux-сервер з Docker Engine (v24+) і Docker Compose v2.
   - Runner має мати мітки `self-hosted`, `linux`, `docker`.
   - Рекомендація: окремий системний користувач без повного shell-доступу, щоб ізолювати деплой.
2. **Доступ до GitHub Container Registry**
   - Створи PAT із правами `write:packages` (або використай `GITHUB_TOKEN`).
   - Додай секрети в репозиторій / environment:
     - `GHCR_TOKEN` – PAT або окремий токен для push у регістр.
     - `GHCR_USERNAME` – якщо PAT виданий не на автора релізу; інакше використається `${{ github.actor }}`.
3. **Конфігурація середовища**
   - На сервері в каталозі репозиторію повинен існувати `deploy/.env`.
   - Початковий шаблон: `deploy/.env.example`. Обов’язково перевизнач `POSTGRES_*`, `JWT_*`, `CORS_ALLOWED_ORIGINS` тощо.
   - Налаштуй права доступу (наприклад, `chmod 600 deploy/.env`).
4. **Persisted volumes та логи**
   - Директорія `logs/` монтується в контейнер API.
   - Том `postgres-data` визначено в `deploy/docker-compose.yml`; на сервері не видаляй його, якщо потрібна історія БД.

## 2. Автоматичний релізний пайплайн

Файл: `.github/workflows/release-docker-deploy.yml`

1. **Тригер**
   - `release.published`: автоматично після публікації релізу на GitHub.
   - `workflow_dispatch`: ручний запуск з параметрами `tag` (обов’язково, якщо перезапуск) та `environment` (за потреби).
2. **Що відбувається**
   1. Визначається тег релізу (наприклад, `v1.2.3`).
   2. Checkout коду на відповідний тег.
   3. Збірка Docker-імеджа з репозиторію.
   4. Пуш імеджа у GHCR з тегами:
      - `ghcr.io/<owner>/<repo>:v1.2.3`
      - `ghcr.io/<owner>/<repo>:latest`
   5. На тому ж runner’і виконується `docker compose -f deploy/docker-compose.yml up -d --pull always --remove-orphans`.
   6. Видаляються непотрібні образи (`docker image prune -f`).
3. **Вихідні артефакти**
   - Конкретний образ у GHCR.
   - Оновлений стек (API + Postgres + pgAdmin) на сервері.

## 3. Як зробити реліз

1. Переконайся, що останній коміт у гілці пройшов CI (`.github/workflows/ci.yml`).
2. Створи семантичний тег і запуш його:
   ```bash
   git tag v1.2.3
   git push origin v1.2.3
   ```
3. В інтерфейсі GitHub:
   1. Перейди на вкладку **Releases → Draft a new release**.
   2. Обери створений тег, додай нотатки, за потреби прикріпи артефакти.
   3. Натисни **Publish release**.
4. Дочекайся завершення workflow:
   - Моніторинг у вкладці **Actions**.
   - Статус деплою відображається у логах кроку “Deploy with Docker Compose”.
5. Перевір сервіс:
   - `docker ps` на сервері – контейнер `komunalka-api` повинен бути у стані `Up`.
   - Запит `curl http://localhost:8080/health` (або через reverse proxy) має повернути успішну відповідь.
   - За потреби, переглянь логи: `docker logs komunalka-api`.

## 4. Повторний або ручний деплой

1. Відкрий workflow `Release Docker Deploy`.
2. Обери **Run workflow**, задай:
   - `tag` – існуючий реліз (наприклад, `v1.2.3`).
   - `environment` – значення збігається з назвою GitHub Environment (за замовчуванням `production`).
3. Запуск відтворить кроки збірки й деплою без створення нового релізу.

## 5. Оновлення конфігурації

- Щоб змінити секрети або параметри БД, відредагуй `deploy/.env` і перезапусти workflow вручну або виконай на сервері:
  ```bash
  docker compose -f deploy/docker-compose.yml up -d
  ```
- Для зміни портів чи додаткових сервісів внось правки у `deploy/docker-compose.yml`.
- Якщо виправляєш `Dockerfile`, обов’язково публікуй новий реліз або повторно запускай workflow, інакше runner буде використовувати попередній образ.

## 6. Траблшутінг

| Симптом | Можливі причини | Дії |
|---------|----------------|-----|
| Workflow падає на логіні у GHCR | Неправильний `GHCR_TOKEN` або відсутні права `write:packages` | Перегенеруй PAT, перевір секрети, переконайся, що runner бачить `$GHCR_TOKEN` |
| `docker compose` завершується з помилкою змінних | Відсутній або неповний `deploy/.env` | Зрівняй із `deploy/.env.example`, перевір права доступу |
| API не стартує після деплою | Невірні параметри БД/JWT, проблема міграцій | Перевір логи `komunalka-api`, застосуй міграції вручну, онови секрети |
| Runner не знаходить Docker | Відсутній Docker або сервіс не в групі `docker` | Встанови Docker, додай користувача runner’а до групи `docker` |

---

**Рекомендації:** регулярно роби `docker system prune` на сервері (або налаштуй cron) і перевіряй, що GHCR не забитий застарілими образами. Пам’ятай, що `deploy/.env`—єдине джерело чутливих даних для продакшену; зберігай резервну копію в секретному сховищі.
