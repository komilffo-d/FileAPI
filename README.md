# FileAPI

Продвинутое WEB API для загрузки больших файлов с возможностями отслеживания прогресса, создания токенов для загрузки элементов сторонними пользователями.

## Установка
Для полноценного развёртывания необходимы image-снимки postgreSQL и текущего проекта в Docker type.

### Docker Postgres
Использовать Docker-image [postgresql 16](https://hub.docker.com/_/postgres) для добавления СУБД.
```bash
docker pull postgres:16
```
> [!IMPORTANT]
> Необходимо устанавливать версию PostgreSQL от 12 и выше версии
### Docker WEB API
В корневой папке проекта выполнить следующую команду для формирования снимка:
```bash
docker build -t <наименование снимка> .
```
> [!NOTE]
> Также можно выгружать из архива tar-типа командой "docker load -i <наименование архива.tar>
### Запуска Docker-контейнера
Пример файла docker-compose.yml, который воспроизводит дальнейшую сборку изображений и запуск их в среде с необходимыми параметрами.
```bash
version: '3'
services:
  webapi:
    container_name: webapi
    restart: always
    build:
      context: ./
      dockerfile: Dockerfile
    ports:
      - "8080:80"
    depends_on:
      - db
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=database;Username=admin;Password=admin;"
  db:
    container_name: db
    image: postgres:16
    environment:
      - POSTGRES_USER=admin
      - POSTGRES_PASSWORD=admin
      - POSTGRES_DB=database
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata: {}
```
```bash
docker compose up --build
```
## Вывод

Используйте с умом сие творение.

## License

[MIT](https://choosealicense.com/licenses/mit/)
