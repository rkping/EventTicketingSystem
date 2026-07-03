# Event Ticketing System API

A RESTful API for a simplified event ticketing system using Clean Architecture, CQRS with MediatR, FluentValidation, EF Core, and SQLite.

## Features

- Create, retrieve, update, and delete events
- Purchase tickets for an event
- View ticket availability
- Prevent ticket overselling
- Generate ticket sales summary by event
- Validation and global error handling
- Unit/integration test ready design

## Technology Stack

- .NET / ASP.NET Core Web API
- Clean Architecture
- CQRS with MediatR
- FluentValidation
- Entity Framework Core
- SQLite
- Repository Pattern
- Unit of Work
- Transaction Runner
- Swagger / OpenAPI
- xUnit, Moq, FluentAssertions, coverlet

## Solution Structure

```text
EventTicketingSystem
├── EventTicketing.Api
│   ├── Controllers
│   ├── Middleware
│   ├── Data
│   ├── Program.cs
│   └── appsettings.json
│
├── EventTicketing.Application
│   ├── Abstractions
│   ├── Events
│   │   ├── Commands
│   │   └── Queries
│   ├── Tickets
│   │   ├── Commands
│   │   └── Queries
│   ├── Reports
│   └── Validators
│
├── EventTicketing.Domain
│   ├── Entities
│   └── Exceptions
│
├── EventTicketing.Infrastructure
│   ├── Data
│   ├── Repositories
│   └── Services
│
└── EventTicketing.Tests
    ├── Api
    ├── Application
    ├── Domain
    ├── Infrastructure
    └── Common
```

## Architecture

The solution follows Clean Architecture principles.

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application / Domain
```

### API Layer

Responsible for HTTP endpoints, Swagger/OpenAPI, request/response handling, and exception middleware.

### Application Layer

Responsible for CQRS commands and queries, MediatR handlers, validators, and abstractions such as repositories, unit of work, and transaction runner interfaces.

### Domain Layer

Responsible for entities, business rules, and domain exceptions.

### Infrastructure Layer

Responsible for EF Core DbContext, SQLite setup, repository implementations, transaction handling, and ticket inventory persistence logic.

## API Endpoints

### Events

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/events` | Create event |
| GET | `/api/events/{eventId}` | Get event by ID |
| GET | `/api/events` | List events |
| PUT | `/api/events/{eventId}` | Update event |
| DELETE | `/api/events/{eventId}` | Delete event |

### Tickets

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/events/{eventId}/tickets/purchase` | Purchase tickets |
| GET | `/api/events/{eventId}/availability` | View ticket availability |

### Reports

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/reports/events/{eventId}/ticket-sales` | Ticket sales summary by event |

## Database

The project uses SQLite for local development.

Default connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=Data/ticketing.db"
  }
}
```

The SQLite database is created under:

```text
EventTicketing.Api/Data/ticketing.db
```

SQLite database files should not be committed to Git.

Recommended `.gitignore` entries:

```gitignore
*.db
*.db-shm
*.db-wal
**/Data/*.db
**/Data/*.db-shm
**/Data/*.db-wal
!**/Data/.gitkeep
bin/
obj/
.vs/
```

Because Git does not commit empty folders, keep this file in the repository:

```text
EventTicketing.Api/Data/.gitkeep
```

## Local Setup

### Prerequisites

Install:

- .NET SDK
- Visual Studio or Visual Studio Code
- Git
- Optional: SQLite browser tool

Verify .NET:

```bash
dotnet --version
```

### Clone the repository

```bash
git clone https://github.com/rkping/EventTicketingSystem.git
cd EventTicketingSystem
```

### Restore packages

```bash
dotnet restore
```

### Ensure Data folder exists

If the folder does not exist:

```bash
mkdir EventTicketing.Api\Data
```

### Run the API

```bash
dotnet run --project EventTicketing.Api
```

### Open Swagger

Use the URL shown in the console, usually:

```text
https://localhost:<port>/swagger
```

or:

```text
http://localhost:<port>/swagger
```

## Important SQLite Setup Note

If the project uses:

```csharp
dbContext.Database.EnsureCreated();
```

the schema is created directly from the EF Core model.

If the project uses:

```csharp
dbContext.Database.Migrate();
```

then EF Core migrations must exist under:

```text
EventTicketing.Infrastructure/Migrations
```

For this interview/local project, `EnsureCreated()` is acceptable for quick setup. For production or long-term development, prefer EF Core migrations.

## Sample Create Event Request

```http
POST /api/events
Content-Type: application/json
```

```json
{
  "name": "Coldplay Live",
  "description": "Music concert",
  "venue": "Dallas Stadium",
  "eventDate": "2026-12-20",
  "eventTime": "19:00:00",
  "totalCapacity": 100,
  "pricingTiers": [
    {
      "name": "General",
      "price": 50,
      "capacity": 70
    },
    {
      "name": "VIP",
      "price": 150,
      "capacity": 30
    }
  ]
}
```

Expected response:

```http
201 Created
```

Copy the returned `id` and use it for other endpoints.

## Get Event by ID

```http
GET /api/events/{eventId}
```

If the event does not exist:

```http
404 Not Found
```

## View Ticket Availability

This endpoint satisfies the requirement: **View ticket availability**.

```http
GET /api/events/{eventId}/availability
```

Example response:

```json
{
  "eventId": "event-id",
  "totalCapacity": 100,
  "soldTickets": 20,
  "availableTickets": 80,
  "pricingTiers": [
    {
      "pricingTierId": "tier-id",
      "name": "General",
      "capacity": 70,
      "soldQuantity": 10,
      "availableQuantity": 60
    },
    {
      "pricingTierId": "tier-id",
      "name": "VIP",
      "capacity": 30,
      "soldQuantity": 10,
      "availableQuantity": 20
    }
  ]
}
```

## Purchase Tickets

```http
POST /api/events/{eventId}/tickets/purchase
Content-Type: application/json
```

```json
{
  "pricingTierId": "pricing-tier-id",
  "buyerName": "Ravi Kumar",
  "buyerEmail": "ravi@example.com",
  "quantity": 2
}
```

If tickets are unavailable:

```http
409 Conflict
```

## Ticket Inventory Concurrency Strategy

To prevent overselling, ticket reservation uses an atomic conditional update:

```sql
UPDATE PricingTiers
SET SoldQuantity = SoldQuantity + @quantity,
    Version = Version + 1
WHERE Id = @pricingTierId
  AND EventId = @eventId
  AND SoldQuantity + @quantity <= Capacity
```

Result handling:

```text
affectedRows == 1  => reservation succeeded
affectedRows == 0  => not enough tickets or invalid event/tier
```

This protects inventory even when multiple buyers purchase tickets concurrently.

## Validation

The project uses FluentValidation.

Common validation rules:

- Event name is required
- Description is required
- Venue is required
- Total capacity must be greater than zero
- Pricing tiers are required
- Pricing tier capacity must be greater than zero
- Pricing tier price cannot be negative
- Sum of pricing tier capacities cannot exceed event total capacity
- Buyer name is required
- Buyer email must be valid
- Purchase quantity must be greater than zero

Validation failures should return:

```http
400 Bad Request
```

## Error Handling

The API uses custom exceptions and middleware to return consistent HTTP responses.

| Exception | HTTP Status |
|---|---|
| ValidationException | 400 Bad Request |
| DomainValidationException | 400 Bad Request |
| NotFoundException | 404 Not Found |
| ConflictException | 409 Conflict |
| Unexpected exception | 500 Internal Server Error |

Example response:

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Event not found.",
  "instance": "/api/events/{eventId}"
}
```

## Transactions

The project uses Unit of Work and transaction runner patterns.

For simple operations:

```csharp
await _repository.AddAsync(entity, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
```

For explicit transaction control:

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async ct =>
{
    await _repository.AddAsync(entity, ct);
    await _unitOfWork.SaveChangesAsync(ct);
}, cancellationToken);
```

For operations returning a value:

```csharp
var result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
{
    // business logic
    return response;
}, cancellationToken);
```

Ticket purchase should use explicit transaction handling because it includes event lookup, tier lookup, inventory reservation, purchase insert, and save.

## Running Tests

Run all tests:

```bash
dotnet test
```

Run tests with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Recommended single test project structure:

```text
EventTicketing.Tests
├── Api
│   ├── Controllers
│   └── Middleware
├── Application
│   ├── Events
│   ├── Tickets
│   ├── Reports
│   └── Validators
├── Domain
│   └── Entities
├── Infrastructure
│   ├── Repositories
│   ├── Services
│   └── Data
└── Common
    ├── Builders
    ├── Fixtures
    └── TestData
```

Important test coverage areas:

- Domain entity rules
- FluentValidation validators
- MediatR handlers
- Repository methods
- Ticket inventory service
- Transaction rollback behavior
- Exception middleware
- Concurrent purchase overselling protection

## Git Setup

Initialize Git:

```bash
git init
```

Add files:

```bash
git add .
```

Commit:

```bash
git commit -m "Initial commit - Event Ticketing API"
```

Add remote:

```bash
git remote add origin https://github.com/rkping/EventTicketingSystem.git
```

Push:

```bash
git branch -M main
git push -u origin main
```

For future changes:

```bash
git add .
git commit -m "Your commit message"
git push
```

## Fresh Clone Troubleshooting

### SQLite cannot open database file

If you see an error like:

```text
An error occurred using the connection to database 'main' on server 'Data/ticketing.db'
```

create the Data folder:

```bash
mkdir EventTicketing.Api\Data
```

Also make sure this file exists in Git:

```text
EventTicketing.Api/Data/.gitkeep
```

### SQLite Error: no such table: Events

If using migrations:

```bash
dotnet ef database update --project EventTicketing.Infrastructure --startup-project EventTicketing.Api
```

If using `EnsureCreated()`, delete old DB files and restart the API:

```text
EventTicketing.Api/Data/ticketing.db
EventTicketing.Api/Data/ticketing.db-shm
EventTicketing.Api/Data/ticketing.db-wal
```

Then run:

```bash
dotnet run --project EventTicketing.Api
```

### dotnet ef command not found

Install EF Core CLI:

```bash
dotnet tool install --global dotnet-ef
```

Verify:

```bash
dotnet ef --version
```

## Production Azure Recommendation

For local and interview evaluation, SQLite is used for simplicity.

For production, recommended Azure architecture:

| Need | Azure Service |
|---|---|
| API hosting | Azure App Service or Azure Container Apps |
| Database | Azure SQL Database or PostgreSQL Flexible Server |
| Secrets | Azure Key Vault |
| Monitoring | Application Insights |
| Global routing/WAF | Azure Front Door |
| Async purchase processing | Azure Service Bus |
| Read caching | Azure Cache for Redis |

Production improvements:

- Replace SQLite with Azure SQL or PostgreSQL
- Keep the atomic inventory update pattern
- Add idempotency keys for purchase requests
- Add authentication and authorization
- Add structured logging and distributed tracing
- Run database migrations through CI/CD

## Design Decisions

| Decision | Reason |
|---|---|
| Clean Architecture | Separates API, Application, Domain, and Infrastructure concerns |
| CQRS with MediatR | Keeps commands and queries clean and testable |
| FluentValidation | Centralized request validation |
| SQLite | Easy local setup |
| EF Core | Productive data access |
| Repository Pattern | Keeps Application independent from Infrastructure |
| Unit of Work | Clear save/commit boundary |
| Atomic SQL update | Prevents overselling |
| ProblemDetails middleware | Consistent API errors |

## Summary

This solution demonstrates:

- Clean Architecture
- CQRS with MediatR
- FluentValidation
- EF Core with SQLite
- Transaction-safe ticket purchasing
- Overselling protection
- Global exception handling
- Testing and code coverage readiness
- Production-aware design trade-offs
