# English Learning Platform - Backend API

Clean Architecture implementation with .NET 9, PostgreSQL/Supabase, JWT Auth, CQRS, and Domain-Driven Design.

## 🏗️ Architecture

```
src/
├── EnglishLearning.Domain          # 🟢 Core domain (entities, value objects, events, services)
├── EnglishLearning.Application     # 🟡 CQRS (MediatR), Validators, DTOs, Handlers
├── EnglishLearning.Infrastructure  # 🔵 EF Core, PostgreSQL, Repositories, JWT, Auth
└── EnglishLearning.Api             # 🔴 Controllers, Middleware, Swagger, DI
```

## 🚀 Quick Start (Local Development)

### Prerequisites
- .NET 9 SDK
- Docker & Docker Compose (for PostgreSQL)
- Supabase account (for production)

### 1. Start Local PostgreSQL
```bash
docker-compose up -d
```
This starts:
- PostgreSQL 16 on `localhost:5432` (user: postgres, pass: postgres, db: englishlearning)
- pgAdmin on `http://localhost:5050` (admin@englishlearning.com / admin)

### 2. Run Migrations
```bash
dotnet ef database update --project EnglishLearning.Infrastructure --startup-project EnglishLearning.Api
```

### 3. Run the API
```bash
dotnet run --project EnglishLearning.Api
```

### 4. Open Swagger
Navigate to `https://localhost:7228/swagger`

## 🔐 Supabase Production Setup

### 1. Create Supabase Project
1. Go to [supabase.com](https://supabase.com) and create a new project
2. Note your project reference (e.g., `qwccgaomzkzgukudhpdx`)

### 2. Get Database Password
1. In Supabase Dashboard → Settings → Database
2. Copy the **database password** (not your account password!)
3. This is the password for the `postgres` database user

### 3. Configure Connection Strings
Update `appsettings.Production.json` or set environment variables:

```json
{
  "ConnectionStrings": {
    "Supabase": "Host=aws-0-us-east-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.{PROJECT_REF};Password={YOUR_DB_PASSWORD};Pooling=false;CommandTimeout=300;Timeout=300;",
    "DefaultConnection": "Host=localhost;Database=englishlearning;Username=postgres;Password=postgres;Port=5432"
  }
}
```

Or use the direct connection (port 5432):
```json
"Supabase": "Host=db.{PROJECT_REF}.supabase.co;Port=5432;Database=postgres;Username=postgres;Password={YOUR_DB_PASSWORD};SSL Mode=Require;Pooling=true;MinPoolSize=2;MaxPoolSize=20;"
```

### 4. JWT Configuration
```json
"Jwt": {
  "SecretKey": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS",
  "Issuer": "EnglishLearning.Api",
  "Audience": "EnglishLearning.Client",
  "AccessTokenExpirationMinutes": 15,
  "RefreshTokenExpirationDays": 7
}
```

### 5. Run Migrations on Supabase
```bash
dotnet ef database update --project EnglishLearning.Infrastructure --startup-project EnglishLearning.Api
```

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/auth/register` | Register new user |
| POST | `/api/v1/auth/login` | Login (returns access + refresh tokens) |
| POST | `/api/v1/auth/refresh` | Refresh access token |
| POST | `/api/v1/auth/revoke` | Revoke refresh token |
| GET | `/api/v1/auth/me` | Get current user profile |

### Assessments (Placement Test)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/assessments/questions` | Get diagnostic questions |
| POST | `/api/v1/assessments/submit` | Submit answers, get level |
| GET | `/api/v1/assessments/progress/{userId}` | Get progress report |

### Lessons
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/lessons/by-level/{A1\|A2\|B1\|B2\|C1}` | Get lessons by level |
| GET | `/api/v1/lessons/{id}` | Get lesson with exercises |
| GET | `/api/v1/lessons/exercises/{id}` | Get exercise detail |
| POST | `/api/v1/lessons/evaluate` | Evaluate exercise answer |

### Health Check
| Method | Endpoint |
|--------|----------|
| GET | `/health` |

## 🧪 Testing
```bash
dotnet test EnglishLearning.Tests/EnglishLearning.Tests.csproj
```

## 🐳 Docker Deployment
```bash
# Build
docker build -t englishlearning-api .

# Run with docker-compose (includes PostgreSQL)
docker-compose up -d
```

## 🔧 Configuration Reference

### appsettings.json Sections
```json
{
  "ConnectionStrings": { "Supabase": "...", "DefaultConnection": "..." },
  "Jwt": { "SecretKey": "...", "Issuer": "...", "Audience": "...", "AccessTokenExpirationMinutes": 15, "RefreshTokenExpirationDays": 7 },
  "LevelCalculation": { "Thresholds": { "A1": 0, "A2": 26, "B1": 46, "B2": 66, "C1": 86 }, "PromotionRules": { "MinCorrectPercentage": 80, "MinTotalResponses": 10, "MaxLevel": "C1" } },
  "Assessment": { "MinQuestions": 8, "MaxQuestions": 20, "PassingScorePercentage": 60 },
  "ExerciseEvaluation": { "DefaultStrategy": "CaseInsensitiveExact", "Strategies": { "CaseInsensitiveExact": {}, "Fuzzy": { "Threshold": 0.85 }, "Regex": {} } },
  "Cors": { "AllowedOrigins": ["http://localhost:4200"] },
  "SeedData": { "AutoSeedOnStartup": true, "DataPath": "Seed/Data" }
}
```

## 🎯 Key Features Implemented

- ✅ **Clean Architecture** with strict SOLID principles
- ✅ **CQRS + MediatR** for separation of commands/queries
- ✅ **Domain-Driven Design** (Entities, Value Objects, Domain Events, Aggregates)
- ✅ **JWT Authentication** with Access + Refresh tokens, rotation, revocation
- ✅ **BCrypt Password Hashing**
- ✅ **FluentValidation** + Pipeline Behaviors
- ✅ **Mapster** for object mapping
- ✅ **PostgreSQL/Supabase** with EF Core, JSONB, proper indexing
- ✅ **Unit of Work** pattern with transaction management
- ✅ **Seed Data** service (auto-seeds demo user + 16 diagnostic questions)
- ✅ **Swagger/OpenAPI** with JWT Bearer auth
- ✅ **Health Checks** (PostgreSQL)
- ✅ **Rate Limiting** (AspNetCoreRateLimit)
- ✅ **Security Headers** middleware
- ✅ **Correlation ID** tracking
- ✅ **Global Exception Handling** (RFC 7807 ProblemDetails)
- ✅ **Docker** support (API + PostgreSQL + pgAdmin)

## 📝 Next Steps
- [ ] Add Role-based Authorization (Admin/Teacher/Student)
- [ ] Integration Tests with Testcontainers
- [ ] CI/CD Pipeline (GitHub Actions)
- [ ] Frontend Angular Integration
- [ ] SignalR for Real-time Features
- [ ] OpenTelemetry + Distributed Tracing