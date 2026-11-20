# Microservices lab (STP, lab 4)

Two small ASP.NET Core Web API services that talk to each other over HTTP.

- UserService (port 5001) keeps users in its own Postgres database.
- OrderService (port 5002) keeps orders and asks UserService whether a user exists before saving an order.

Each service has its own database, so OrderService never reads the users table directly.

## Running

I run everything in Docker inside WSL (Ubuntu):

```bash
cd /mnt/c/uni/Microservices
docker compose up -d --build
```

Swagger is at http://localhost:5001/swagger and http://localhost:5002/swagger.

To stop and wipe the data:

```bash
docker compose down -v
```

You can also start only the databases (`docker compose up -d users-db orders-db`) and run the services with `dotnet run`. In that case they use the connection strings from `appsettings.json`, ports 5433 and 5434 (5432 was already taken on my machine).

## API

UserService has the usual CRUD on `/api/users`.

OrderService has CRUD on `/api/orders` plus two endpoints that call UserService:

- `GET /api/orders/user/{userId}` returns the user as UserService sees it
- `GET /api/orders/{id}/details` returns the order together with its user

Creating or updating an order for a user that doesn't exist gives 400. If UserService is down, OrderService answers 503 instead of hanging.

## Config

The UserService address lives in `OrderService/appsettings.json` under `Services:UserServiceUrl`. Inside docker-compose it's overridden with the `Services__UserServiceUrl` env variable, because there the containers reach each other by name (`http://user-service:8080/`).
