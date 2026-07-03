# EventTicketing.Tests

Comprehensive test suite for the Event Ticketing System using xUnit, FluentAssertions, and Moq.

## Test Structure

```
EventTicketing.Tests/
??? Api/
?   ??? Controllers/
?   ?   ??? EventsControllerIntegrationTests.cs
?   ??? Middleware/
?       ??? ExceptionHandlingMiddlewareTests.cs
??? Application/
?   ??? Events/
?   ?   ??? Commands/
?   ?   ?   ??? CreateEventCommandHandlerTests.cs
?   ?   ??? Queries/
?   ?       ??? GetEventByIdQueryHandlerTests.cs
?   ?       ??? ListEventsQueryHandlerTests.cs
?   ??? Reports/
?   ?   ??? GetTicketSalesReportQueryHandlerTests.cs
?   ??? Tickets/
?   ?   ??? Commands/
?   ?   ?   ??? PurchaseTicketsCommandHandlerTests.cs
?   ?   ??? Queries/
?   ?       ??? GetEventAvailabilityQueryHandlerTests.cs
?   ??? Validators/
?       ??? CreateEventCommandValidatorTests.cs
?       ??? PurchaseTicketsCommandValidatorTests.cs
??? Domain/
?   ??? Entities/
?       ??? EventTests.cs
?       ??? PricingTierTests.cs
?       ??? TicketPurchaseTests.cs
??? Infrastructure/
?   ??? Repositories/
?   ?   ??? EventRepositoryTests.cs
?   ?   ??? PricingTierRepositoryTests.cs
?   ?   ??? TicketPurchaseRepositoryTests.cs
?   ??? Services/
?       ??? TicketInventoryServiceTests.cs
??? Common/
    ??? Builders/
    ?   ??? CreateEventCommandBuilder.cs
    ?   ??? EventBuilder.cs
    ?   ??? PricingTierBuilder.cs
    ?   ??? PurchaseTicketsCommandBuilder.cs
    ?   ??? TicketPurchaseBuilder.cs
    ??? Fixtures/
        ??? DbContextFixture.cs
```

## Running Tests

### Run All Tests
```powershell
dotnet test
```

### Run Tests with Code Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

The code coverage report will be generated in:
```
EventTicketing.Tests/bin/Debug/net10.0/coverage.cobertura.xml
```

### Run Tests with Verbose Output
```powershell
dotnet test --verbosity normal
```

### Run Tests for Specific Category
```powershell
# Domain tests only
dotnet test --filter "FullyQualifiedName~EventTicketing.Tests.Domain"

# Application tests only
dotnet test --filter "FullyQualifiedName~EventTicketing.Tests.Application"

# Infrastructure tests only
dotnet test --filter "FullyQualifiedName~EventTicketing.Tests.Infrastructure"

# API tests only
dotnet test --filter "FullyQualifiedName~EventTicketing.Tests.Api"
```

### Run Tests for Specific Handler/Validator
```powershell
# CreateEventCommandHandler tests
dotnet test --filter "FullyQualifiedName~CreateEventCommandHandlerTests"

# PurchaseTicketsValidator tests
dotnet test --filter "FullyQualifiedName~PurchaseTicketsCommandValidatorTests"
```

## Test Coverage Summary

### Domain Layer (3 test files, 30+ tests)
- **Event Entity**: Constructor, AddPricingTier, validation, edge cases
- **PricingTier Entity**: Constructor, AvailableQuantity, EnsureCanPurchase, validation
- **TicketPurchase Entity**: Constructor, TotalAmount calculation, validation

### Application Layer (10 test files, 40+ tests)
- **Validators**: CreateEventCommandValidator, PurchaseTicketsCommandValidator
- **CQRS Handlers**: 
  - CreateEventCommandHandler
  - GetEventByIdQueryHandler
  - ListEventsQueryHandler
  - PurchaseTicketsCommandHandler
  - GetEventAvailabilityQueryHandler
  - GetTicketSalesReportQueryHandler

### Infrastructure Layer (4 test files, 25+ tests)
- **Repositories**: EventRepository, PricingTierRepository, TicketPurchaseRepository
- **Services**: TicketInventoryService (including concurrency tests)
- **In-Memory SQLite**: All tests use in-memory database for isolation

### API Layer (2 test files, 15+ tests)
- **Controllers**: EventsController integration tests
- **Middleware**: ExceptionHandlingMiddleware tests
- **Status Codes**: 201 Created, 400 Bad Request, 404 Not Found, 409 Conflict, 500 Server Error

## Test Naming Convention

All tests follow the **Arrange-Act-Assert** (AAA) pattern and use the naming convention:

```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `Handle_ValidCommand_CreatesEvent`
- `Handle_EventDoesNotExist_ThrowsNotFoundException`
- `TryReserveTicketsAsync_ConcurrentRequests_DoesNotOversell`

## Test Utilities

### Builders (Test Data Factories)
Fluent builders for constructing test objects with sensible defaults:

```csharp
var @event = new EventBuilder()
    .WithName("Custom Event")
    .WithTotalCapacity(500)
    .Build();

var command = new CreateEventCommandBuilder()
    .WithName("Conference")
    .WithPricingTiers(
        new PricingTierRequest("VIP", 200m, 100),
        new PricingTierRequest("Standard", 100m, 400))
    .Build();
```

### DbContextFixture
Provides isolated in-memory SQLite database for infrastructure tests:

```csharp
public sealed class RepositoryTests : IAsyncLifetime
{
    private readonly DbContextFixture _fixture = new();
    private TicketingDbContext DbContext => _fixture.DbContext;

    public async Task InitializeAsync() => await _fixture.InitializeAsync();
    public async Task DisposeAsync() => await _fixture.DisposeAsync();
}
```

## Key Testing Features

### Domain Tests
- Constructor validation
- Business rule enforcement
- Edge cases (zero capacity, negative prices, etc.)
- Aggregate integrity

### Application Tests
- CQRS command/query handling
- Validation logic
- Transaction management
- Exception handling
- Repository mocking

### Infrastructure Tests
- Real database operations (in-memory SQLite)
- Repository behavior
- Concurrency handling
- Data persistence
- Concurrent ticket reservation (50 parallel tasks vs 10 capacity)

### API Tests
- HTTP status codes (201, 400, 404, 409, 500)
- JSON serialization/deserialization
- Integration with handlers
- WebApplicationFactory for testing

### Middleware Tests
- Exception transformation to ProblemDetails
- Status code mapping
- Message redaction for internal errors
- Response stream handling

## Concurrency Edge Case

The TicketInventoryService includes a critical concurrency test:

```csharp
[Fact]
public async Task TryReserveTicketsAsync_ConcurrentRequests_DoesNotOversell()
{
    // Given: Event has one pricing tier with capacity 10
    // When: 50 parallel tasks attempt to reserve 1 ticket each
    // Then: Exactly 10 reservations succeed, SoldQuantity equals 10
}
```

This ensures the system safely handles concurrent ticket purchases using SQL-based atomic updates.

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions**: Fluent assertion library
- **Moq**: Mocking framework
- **Microsoft.EntityFrameworkCore.Sqlite**: In-memory SQLite
- **Microsoft.AspNetCore.Mvc.Testing**: API testing
- **coverlet.collector**: Code coverage collection

## Notes

- All tests are isolated and can run in any order
- No external services are used
- No real database files are created
- Each infrastructure test gets a fresh in-memory database
- Tests follow AAA (Arrange-Act-Assert) pattern
- Meaningful assertions go beyond null checks
- Both success and failure paths are tested
