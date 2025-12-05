# User API Demo

A comprehensive .NET 9 API project demonstrating clean architecture principles with authentication, user management, caching, and Docker support.

## Architecture Overview

The project follows **Clean Architecture** principles with the following layers:

```
UserApiDemo/
├── UserApiDemo.API              # Presentation Layer (Controllers, Program.cs)
├── UserApiDemo.Domain           # Domain Layer (Entities, Interfaces)
├── UserApiDemo.Application      # Application Layer (DTOs, Services, Business Logic)
├── UserApiDemo.Infrastructure   # Infrastructure Layer (External Services, Middleware)
└── UserApiDemo.Persistence      # Persistence Layer (DbContext, Repositories)
```

## Features

- **Authentication & Authorization**
  - JWT-based token authentication
  - User registration and login endpoints
  - Secure password hashing with BCrypt

- **User Management**
  - CRUD operations for users
  - User profile filtering by name
  - Filter users by date of birth (month and day)
  - Automatic database migrations on startup

- **Caching**
  - Redis integration for caching user data
  - Cache invalidation on data updates
  - 30-minute cache expiration

- **Database**
  - SQL Server with Entity Framework Core Code-First approach
  - Automatic migrations on application startup
  - Database constraints and indexes

- **Logging & Error Handling**
  - Serilog for structured logging
  - Global exception handling middleware
  - Request/response logging middleware

- **Docker Support**
  - Multi-container setup with Docker Compose
  - Development and production configurations
  - Remote debugging capability for development containers

## Prerequisites

- .NET 9 SDK
- Docker and Docker Compose
- SQL Server 2022 (or use the Docker image)
- Redis 7 (or use the Docker image)

## Project Setup

### Option 1: Using Docker (Recommended)

#### Production Environment

```bash
# Clone the repository
cd user-api-demo

# Start services
docker-compose up -d

# Check logs
docker-compose logs -f api

# Stop services
docker-compose down
```

The API will be available at `http://localhost:8080`

#### Development Environment

```bash
# Start development containers
docker-compose -f docker-compose.dev.yml up -d

# Check API logs
docker-compose -f docker-compose.dev.yml logs -f api

# For live file changes, the API will automatically restart (dotnet watch)

# Stop development containers
docker-compose -f docker-compose.dev.yml down
```

### Option 2: Local Development

#### Prerequisites

- SQL Server running locally on `localhost,1433`
- Redis running on `localhost:6379`
- Connection string configured in `appsettings.Development.json`

#### Run the application

```bash
cd UserApiDemo.API
dotnet restore
dotnet run
```

## Configuration

### appsettings.json

Update database and Redis connection strings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=UserApiDemo;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;",
    "Redis": "redis:6379"
  },
  "JwtSettings": {
    "SecretKey": "your-very-secret-key-that-is-long-enough-for-jwt-algorithm",
    "Issuer": "UserApiDemo",
    "Audience": "UserApiDemo.Users",
    "ExpirationMinutes": 1440
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Set to `Development` or `Production`
- `ASPNETCORE_URLS`: API binding URL (default: `http://+:8080`)

## API Endpoints

### Authentication

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-05-15"
}
```

Response:
```json
{
  "success": true,
  "message": "User registered successfully",
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john.doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-05-15T00:00:00",
    "isActive": true,
    "createdAt": "2024-12-05T10:30:00"
  }
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

### Users (Requires Authentication)

Include JWT token in header:
```
Authorization: Bearer <your_jwt_token>
```

#### Get All Users
```http
GET /api/users
```

#### Get User by ID
```http
GET /api/users/{id}
```

#### Get User by ID (with Caching)
```http
GET /api/users/{id}/cached
```

#### Filter Users by Name
```http
GET /api/users/filter/name?name=John
```

#### Filter Users by Date of Birth
```http
GET /api/users/filter/dob?month=5&day=15
```

#### Update User
```http
PUT /api/users/{id}
Content-Type: application/json

{
  "firstName": "Jane",
  "lastName": "Smith",
  "dateOfBirth": "1992-03-20"
}
```

#### Delete User
```http
DELETE /api/users/{id}
```

## Remote Debugging in VS Code

### Setup Dev Container

1. Open the project in VS Code
2. Ensure Docker and Docker Compose are running
3. Start development containers:
   ```bash
   docker-compose -f docker-compose.dev.yml up -d
   ```

### Debug Configuration

#### Option 1: Debug Dev Container (Recommended)

1. Open Debug view (`Ctrl+Shift+D` or `Cmd+Shift+D`)
2. Select **"Debug Dev Container API"** from the dropdown
3. Click "Start Debugging" (F5)
4. VS Code will build and launch the API in the container
5. Set breakpoints and step through code

#### Option 2: Debug Locally

Requirements:
- SQL Server 2022 running on `localhost:1433`
- Redis running on `localhost:6379`

Steps:
1. Open Debug view (`Ctrl+Shift+D` or `Cmd+Shift+D`)
2. Select **".NET Core Local Launch"** from the dropdown
3. Click "Start Debugging" (F5)
4. API will start at `http://localhost:5000`

### Useful Debug Tasks

In VS Code, open Command Palette (`Ctrl+Shift+P` or `Cmd+Shift+P`) and select "Tasks: Run Task":

- **docker-build-dev**: Rebuild the API in container
- **docker-logs-api**: View live container logs
- **docker-up-dev**: Start containers
- **docker-down-dev**: Stop containers

### Debug Features

- Set breakpoints in any C# file
- Step into/over/out of code
- Inspect variables and watch expressions
- Evaluate expressions in Debug Console
- View call stack
- Conditional breakpoints

### Debugging Tips

- File changes trigger auto-rebuild with `dotnet watch`
- Breakpoints persist across auto-rebuilds
- Use Debug Console for quick LINQ queries
- Check "VARIABLES" panel to inspect object state
- Use "WATCH" panel to track specific expressions

## Project Structure Details

### Domain Layer (`UserApiDemo.Domain`)
- **Entities**: `User.cs` - Core user entity
- **Interfaces**: Repository and service contracts

### Application Layer (`UserApiDemo.Application`)
- **DTOs**: Data transfer objects for API contracts
- **Services**: Business logic implementation
- **Interfaces**: Service contracts

### Infrastructure Layer (`UserApiDemo.Infrastructure`)
- **Extensions**: Dependency injection setup
- **Middleware**: Exception handling and logging

### Persistence Layer (`UserApiDemo.Persistence`)
- **Data**: Entity Framework DbContext configuration
- **Repositories**: Data access implementation

### API Layer (`UserApiDemo.API`)
- **Controllers**: API endpoints
- **Program.cs**: Application startup configuration
- **appsettings.json**: Configuration files

## Database

### Connection String
- **Server**: SQL Server (Docker: `sqlserver`)
- **Database**: `UserApiDemo`
- **Authentication**: SQL Server Authentication (User: `sa`)

### Auto Migrations
- Migrations run automatically on application startup
- Code-First approach using Entity Framework Core
- Initial schema includes indexes on username, email, and date of birth

## Caching Strategy

### Redis Configuration
- **Host**: `redis:6379` (Docker) or `localhost:6379` (Local)
- **Cache Key Format**: `user_{userId}`
- **Expiration**: 30 minutes

### Cache Operations
- User data is cached after first retrieval
- Cache is invalidated on user updates or deletion
- Separate endpoint for cache-aware retrieval

## Logging

### Serilog Configuration
- **Console**: Structured logging to console output
- **File**: Rolling file logs in `logs/` directory
- **Levels**: Information level and above

### Log Files
```
logs/
├── api-20240101.txt
├── api-20240102.txt
└── ...
```

## Security Considerations

1. **Password Security**: BCrypt hashing with salt
2. **JWT Tokens**: 24-hour expiration, HS256 algorithm
3. **CORS**: Configured to allow all origins (update in production)
4. **HTTPS**: Enforced in production (redirect from HTTP)
5. **SQL Injection**: Protected by Entity Framework parameterized queries

## Development Commands

### Build Solution
```bash
dotnet build
```

### Run Tests (if tests project added)
```bash
dotnet test
```

### Create Migration
```bash
cd UserApiDemo.Persistence
dotnet ef migrations add {MigrationName} -s ../UserApiDemo.API
```

### Update Database
```bash
cd UserApiDemo.Persistence
dotnet ef database update -s ../UserApiDemo.API
```

### Docker Commands

```bash
# View running containers
docker ps

# View container logs
docker logs user-api-dev

# Execute command in container
docker exec -it user-api-dev dotnet ef migrations add {MigrationName}

# Clean up Docker resources
docker-compose down -v
```

## Troubleshooting

### Database Connection Failed
- Ensure SQL Server container is running: `docker ps`
- Check connection string in `appsettings.json`
- Verify SQL Server password matches Docker environment

### Redis Connection Failed
- Ensure Redis container is running
- Check Redis connection string
- Verify port 6379 is not in use

### Debugging Not Working
- Ensure VS Code has C# extension installed
- Check that Docker containers are running with `-d` flag
- Verify port 4711 is not in use for debugger
- Check Docker logs: `docker-compose -f docker-compose.dev.yml logs api`

### Migrations Failed
- Check database connection
- Verify `UserApiDemo.Persistence` is set as startup project
- Clear `bin` and `obj` folders and rebuild

## License

This project is provided as a demonstration of clean architecture principles.

## Support

For issues or questions, refer to the comments in the source code or create an issue in the repository.
