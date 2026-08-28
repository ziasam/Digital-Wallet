# Digital Wallet API

A backend digital wallet API built with **ASP.NET Core / .NET 10**, **Entity Framework Core**, and **PostgreSQL**.

The project demonstrates how to implement wallet operations with transaction safety, concurrency handling, idempotency, JWT-based authentication, and centralized exception handling.

> **Note:** This project is intended as a demonstration/reference implementation. Additional controls and infrastructure would be required before using it in a production financial system.

## Technology Stack

* **.NET 10**
* **ASP.NET Core Web API**
* **Entity Framework Core**
* **PostgreSQL**
* **Npgsql**
* **JWT Bearer Authentication**
* **Swagger / OpenAPI**
* **Custom Exception Handling Middleware**

## Architecture

The project follows a layered structure:

```text
DigitalWalletDemo
│
├── Api
│   ├── Controllers
│   ├── Middleware
│   └── Program.cs
|
├── Application
│   ├── Dtos
|       ├── Authentication
│       └── Wallet
│   ├── Exceptions
│   ├── Interfaces
│   └── Services
│
├── Domain
│   ├── Entities
│   └── Enums
│
├── Infrastructure
│   ├── Authentication
│   └── Data
│       └── Configurations
│
├── Migrations
│
├── appsettings.json
└── appsettings.Development.json
```

### Application

Contains application services, DTOs, interfaces, and business exceptions.

### Domain

Contains the core wallet entities and transaction-related enums.

### Infrastructure

Contains database access and JWT authentication implementation.

### Controllers

Exposes the REST API endpoints.

## Main Features

### 1. JWT Authentication

The API uses JWT Bearer authentication.

The authentication flow is:

```text
Register
   ↓
User created
   ↓
Login
   ↓
JWT generated
   ↓
Authorization header
   ↓
Protected wallet endpoints
```

Protected endpoints require:

```http
Authorization: Bearer <token>
```

JWT configuration is stored in application configuration.

For production, secrets should not be committed to `appsettings.json`.

---

## 2. Wallet Management

The application supports wallet-based balance management.

Wallet operations include:

* Deposit
* Withdrawal
* Wallet-to-wallet transfer
* Balance management
* Currency validation
* Wallet status validation

Wallet operations update the balance and transaction records within database transactions.

---

## 3. Deposit

A deposit:

1. Validates the idempotency key.
2. Finds the user's wallet.
3. Locks the wallet row.
4. Validates the wallet.
5. Validates the amount.
6. Validates the currency.
7. Checks the transaction cooldown.
8. Credits the wallet.
9. Creates a wallet transaction record.
10. Creates an idempotency record.
11. Saves all changes.
12. Commits the database transaction.

Conceptually:

```text
Request
   ↓
Idempotency Check
   ↓
Lock Wallet
   ↓
Validate
   ↓
Balance += Amount
   ↓
Create Transaction
   ↓
Create Idempotency Record
   ↓
Commit
```

---

## 4. Withdrawal

Withdrawal follows the same transaction-safe approach as deposit but additionally checks the available balance.

```text
if Balance < Amount
    → InsufficientBalanceException
```

The operation then:

```text
Balance -= Amount
```

and creates the corresponding transaction and idempotency records within the same database transaction.

This prevents the balance update and transaction record from becoming inconsistent.

---

## 5. Wallet Transfer

Transfers move money between two wallets.

The operation:

```text
Source Wallet
      │
      │ Debit
      ▼
Destination Wallet
      │
      │ Credit
      ▼
```

The transfer:

1. Checks the idempotency key.
2. Finds the source wallet.
3. Finds the destination wallet.
4. Prevents transferring to the same wallet.
5. Locks both wallets.
6. Uses deterministic wallet ordering when acquiring locks.
7. Validates both wallets.
8. Validates currency.
9. Checks transaction cooldown.
10. Checks source balance.
11. Debits the source wallet.
12. Credits the destination wallet.
13. Creates transfer-out transaction.
14. Creates transfer-in transaction.
15. Creates the idempotency record.
16. Saves all changes.
17. Commits the database transaction.

Both wallets are handled inside the same database transaction.

---

# Idempotency

Idempotency is implemented for:

* Deposit
* Withdrawal
* Transfer

Each request contains an `IdempotencyKey`.

For example:

```json
{
  "amount": 100,
  "currency": "BDT",
  "reference": "BANK-12345",
  "idempotencyKey": "7b2d4e3c-..."
}
```

Before processing a transaction, the application checks whether the idempotency key has already been processed.

If it has, the existing transaction result is returned instead of creating another transaction.

### Why this is important

Consider a mobile application making a withdrawal:

```text
Client
  │
  │ POST /withdraw
  ▼
API
  │
  │ Process transaction
  ▼
Database
  │
  │ Success
  ▼
Network timeout
  │
  ▼
Client does not receive response
```

The client may retry the same request.

Without idempotency:

```text
First request  → -100
Retry request  → -100
Final balance  → -200
```

With idempotency:

```text
First request  → -100
Retry request  → Existing transaction
Final balance  → -100
```

The idempotency key therefore allows the client to safely retry requests after network failures.

A unique database constraint should also be maintained on the idempotency key so concurrent requests using the same key cannot create duplicate transactions.

---

# Concurrency and Wallet Locking

Wallet balance operations are protected using database transactions and PostgreSQL row-level locking.

For example, when transferring between two wallets, the application locks both wallet rows using PostgreSQL `FOR UPDATE`.

Conceptually:

```sql
SELECT *
FROM "Wallets"
WHERE "Id" = ANY(...)
ORDER BY "Id"
FOR UPDATE;
```

`FOR UPDATE` prevents another transaction from modifying the locked wallet rows until the current transaction commits or rolls back.

## Deterministic Lock Ordering

Transfers involving two wallets can potentially create a deadlock if two requests acquire locks in opposite orders.

For example:

```text
Transfer A:
Wallet 1 → Wallet 2

Transfer B:
Wallet 2 → Wallet 1
```

The implementation therefore orders wallet IDs before acquiring the locks:

```text
Wallet 1
Wallet 2
```

Both transactions attempt to acquire locks in the same order.

This reduces the possibility of deadlocks caused by inconsistent lock ordering.

---

# Transaction Atomicity

Deposit, withdrawal, and transfer operations use database transactions.

The general pattern is:

```text
BEGIN TRANSACTION
       ↓
Validate
       ↓
Lock required records
       ↓
Update balance
       ↓
Create transaction record
       ↓
Create idempotency record
       ↓
Save changes
       ↓
COMMIT
```

If an exception occurs:

```text
ROLLBACK
```

This ensures that the balance update and corresponding transaction records are committed or rolled back together.

---

# Transaction Cooldown

Wallet operations include a transaction-gap validation.

The wallet stores the time of the last transaction:

```text
LastTransactionAt
```

A new transaction is rejected if it occurs within the configured cooldown period.

This is intended to demonstrate a business rule requiring a minimum gap between wallet operations.

The exact cooldown value can be adjusted according to the application's requirements.

---

# Business-Friendly IDs

The application uses formatted identifiers for business entities.

Examples:

```text
USR-1001
USR-1002

WAL-1001
WAL-1002

TXN-1001
TXN-1002
```

PostgreSQL sequences are used instead of:

```csharp
Count() + 1
```

This is important because `Count() + 1` is unsafe under concurrent requests.

For example, two simultaneous registrations could both calculate:

```text
Count = 1000
Count + 1 = 1001
```

and generate duplicate IDs.

Database sequences provide concurrency-safe numeric values.

---

# Exception Handling

The application uses centralized custom exception handling middleware.

Examples of application exceptions include:

* `WalletException`
* `InsufficientBalanceException`
* `TransactionCooldownException`

The middleware converts exceptions into consistent HTTP responses.

Example:

```json
{
  "error": "INSUFFICIENT_BALANCE",
  "message": "Insufficient wallet balance."
}
```

Unexpected exceptions return a generic response:

```json
{
  "error": "INTERNAL_ERROR",
  "message": "An unexpected error occurred."
}
```

Internal exception details should be logged rather than returned to API clients.

---

# Database

PostgreSQL is used as the primary database and Entity Framework Core is used for data access.

The project contains EF Core migrations under:

```text
Migrations/
```

Database configuration is provided through the connection string:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=DigitalWallet;Username=postgres;Password=..."
  }
}
```

Do not commit real production database credentials to source control.

---

# Running the Project

## Prerequisites

Install:

* .NET 10 SDK
* PostgreSQL
* Git

Verify .NET:

```bash
dotnet --version
```

Verify PostgreSQL is running and accessible.

---

## 1. Clone the repository

```bash
git clone https://github.com/ziasam/Digital-Wallet.git
```

Enter the project directory:

```bash
cd Digital-Wallet
```

If the implementation is on the `demowallet` branch:

```bash
git checkout demowallet
```

---

## 2. Configure PostgreSQL

Create a PostgreSQL database, for example:

```text
DigitalWallet
```

Then update the connection string in:

```text
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=DigitalWallet;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

Replace the credentials with your local PostgreSQL credentials.

---

## 3. Restore dependencies

```bash
dotnet restore
```

---

## 4. Apply EF Core migrations

Run:

```bash
dotnet ef database update
```

If the EF CLI is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Then run:

```bash
dotnet ef database update
```

---

## 5. Run the API

```bash
dotnet run
```

The configured development URLs can be found in:

```text
Properties/launchSettings.json
```

For example:

```text
https://localhost:7027
http://localhost:5002
```

---

# Swagger

When running in the Development environment, Swagger is enabled.

Open:

```text
https://localhost:7027/swagger/index.html
```

or, depending on the active launch profile:

```text
http://localhost:5002/swagger/index.html
```

Swagger can be used to inspect and test the API endpoints.

For protected endpoints, obtain a JWT token through the authentication endpoint and provide it using the Swagger **Authorize** button.

---

# API Testing

The API can be tested using:

* Swagger
* Postman
* `.http` files
* curl

For example, after obtaining a JWT:

```http
Authorization: Bearer <JWT_TOKEN>
```

For transaction endpoints, always generate a unique `IdempotencyKey` for a new business operation.

When retrying the **same** operation because of a network failure, reuse the same idempotency key.

---

# Production Improvements

This project demonstrates important wallet transaction concepts, but a real production financial system would require additional controls.

## 1. Stronger authentication and authorization

Improve authentication with:

* Refresh tokens
* Token rotation
* Short-lived access tokens
* Key rotation
* Strong secret management
* Multi-factor authentication
* Account lockout / brute-force protection
* Role- and permission-based authorization

JWT signing keys should be stored in a secure secret-management system rather than source-controlled configuration.

---

## 2. Stronger transaction state management

Real payment systems generally need more transaction states than simply `Completed`.

For example:

```text
Pending
Processing
Completed
Failed
Reversed
Cancelled
```

This becomes particularly important when interacting with external banks, cards, payment gateways, or other financial institutions.

---

## 3. External payment providers

The current implementation focuses on wallet/database operations.

Production integration with:

* Banks
* Card processors
* Payment gateways
* Mobile financial services

requires handling external transaction references, provider statuses, callbacks/webhooks, reconciliation, and timeout scenarios.

---

## 4. Reconciliation

A production wallet should have reconciliation processes to compare:

```text
Internal wallet transactions
        vs
External provider transactions
```

Discrepancies should be detected and investigated automatically.

---

## 5. Audit trail

Financial applications should maintain an immutable audit trail.

Record information such as:

* User
* Transaction ID
* Request ID
* Idempotency key
* Previous balance
* New balance
* Transaction amount
* Currency
* Timestamp
* Source
* IP address where appropriate
* External provider reference
* Status changes

---

## 6. Monetary precision

Amounts should be handled carefully.

The implementation uses `decimal`, which is appropriate for many .NET financial calculations, but production systems should also define:

* Currency precision
* Rounding rules
* Minimum transaction amounts
* Maximum transaction amounts
* Currency-specific rules

All monetary calculations should follow explicit domain rules.

---

## 7. Database resilience

Production deployment should consider:

* Connection resiliency
* PostgreSQL backups
* Point-in-time recovery
* Replication
* Monitoring
* Database failover
* Connection pooling
* Migration management

---

## 8. Distributed systems considerations

If the wallet is extended to multiple services, additional distributed-system patterns may be required.

Examples:

* Outbox pattern
* Inbox pattern
* Message queues
* Event-driven processing
* Distributed tracing
* Retry policies
* Circuit breakers
* Dead-letter queues

Idempotency should be maintained across service boundaries, not only within a single database.

---

## 9. Observability

Add structured logging and monitoring for:

* Transaction failures
* Database errors
* Authentication failures
* Failed transfers
* Idempotency conflicts
* Deadlocks
* API latency
* Database latency

Production systems should also use metrics and distributed tracing.

---

## 10. Rate limiting

API endpoints should have rate limits to protect against:

* Brute-force login attempts
* Transaction abuse
* Automated requests
* Denial-of-service scenarios

---

## 11. Validation and security

Add comprehensive validation for:

* Amount
* Currency
* Idempotency key
* References
* Wallet ownership
* Transaction limits
* Account status

Sensitive information should never be logged.

---

## 12. Automated testing

The next major improvement should be a comprehensive test suite.

Recommended test levels:

```text
Unit Tests
    ↓
Service Tests
    ↓
Integration Tests
    ↓
Concurrency Tests
    ↓
End-to-End Tests
```

Important scenarios include:

* Duplicate deposit
* Duplicate withdrawal
* Duplicate transfer
* Concurrent withdrawals
* Concurrent deposits
* Concurrent transfers
* Insufficient balance
* Same-wallet transfer
* Currency mismatch
* Wallet freeze
* Transaction cooldown
* Database rollback
* Network retry with the same idempotency key

---

# Current vs Production

| Area              | Current Implementation      | Production Recommendation              |
| ----------------- | --------------------------- | -------------------------------------- |
| API               | ASP.NET Core                | ASP.NET Core + production hosting      |
| Database          | PostgreSQL                  | PostgreSQL HA/backup strategy          |
| ORM               | EF Core                     | EF Core + optimized queries            |
| Authentication    | JWT                         | JWT + refresh/token rotation + MFA     |
| Exceptions        | Custom middleware           | ProblemDetails + centralized logging   |
| Idempotency       | Deposit/Withdraw/Transfer   | Distributed idempotency strategy       |
| Concurrency       | DB transactions + row locks | Thorough concurrency testing           |
| IDs               | PostgreSQL sequences        | Sequences/UUIDs + business IDs         |
| Logging           | Basic application logging   | Structured centralized logging         |
| Monitoring        | Limited                     | Metrics + tracing + alerting           |
| External payments | Not included                | Provider integration + reconciliation  |
| Testing           | Development focused         | Unit + integration + concurrency + E2E |
| Secrets           | Configuration               | Secret manager                         |
| Deployment        | Local development           | CI/CD + containerized/cloud deployment |

---

# Design Goals

The primary goals of this project are to demonstrate:

1. Secure API authentication using JWT.
2. Database-backed wallet management.
3. Atomic balance updates.
4. PostgreSQL row-level locking.
5. Deterministic lock ordering for transfers.
6. Idempotent deposit, withdrawal, and transfer operations.
7. Protection against duplicate transactions caused by client retries.
8. Centralized exception handling.
9. Database-generated business identifiers.
10. A foundation that can be extended toward a production-grade wallet architecture.

## Disclaimer

This project is a technical demonstration and should not be considered a production-ready financial system.

Real financial applications require additional security, compliance, fraud prevention, reconciliation, operational controls, monitoring, disaster recovery, and extensive testing.
