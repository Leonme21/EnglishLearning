# EnglishLearning — Inglés para Hispanohablantes

Plataforma web integral con onboarding obligatorio (Placement Test → A1-C1) y 4 pilares Reading/Listening/Writing/Speaking, enfocada en falsos amigos, pronunciación y transferencias ES→EN.

## Quick Start
```powershell
dotnet restore
dotnet build EnglishLearning.sln -c Release
# SQL Server: configurar ConnectionStrings:DefaultConnection en appsettings.json o UserSecrets
dotnet ef database update --project EnglishLearning.Infrastructure --startup-project EnglishLearning.Api
dotnet run --project EnglishLearning.Api  # https://localhost:5001/openapi/v1.json
dotnet test                               # xUnit EnglishLearning.Tests
```
Angular (Phase 3):
```bash
# tras crear english-learning-web/
npm install
npm run dev    # http://localhost:4200
```

## Commands
| Command | Description |
|---------|-------------|
| `dotnet build` | Build Debug |
| `dotnet build -c Release` | Build Release |
| `dotnet test` | Tests xUnit (9 tests LeccionServicio) |
| `dotnet run --project EnglishLearning.Api` | API + OpenApi |
| `dotnet ef migrations add <Name> -p EnglishLearning.Infrastructure -s EnglishLearning.Api` | Migración |

## Architecture
`Domain (puro) → Application (servicios, Dtos, ToDto) → Infrastructure (EF Core AsNoTracking, repos) → Api (Composition Root, DI, OpenApi, CORS)`  
`EnglishLearning.Tests` referencia Domain+Application con Fakes. Ver `AGENTS.md:28` y ADRs:

- [ADR-001 Capas](docs/decisions/ADR-001-capas-desacopladas.md)
- [ADR-002 Simplicidad KISS](docs/decisions/ADR-002-simplicidad-kiss.md)
- [ADR-003 Angular SPA](docs/decisions/ADR-003-angular-spa.md)
- [ADR-004 OpenApi nativo](docs/decisions/ADR-004-openapi-vs-swashbuckle.md)
- [Spec Refactor](SPEC-refactor-arquitectura.md) + [Plan](tasks/plan.md) + [Todo](tasks/todo.md)

## API Contratos
- `GET /api/lecciones/nivel/{nivel}` → `LeccionDto[]` (enum A1..C1)
- `POST /api/lecciones/evaluar` → `RespuestaDto{UsuarioId>0, EjercicioId>0, RespuestaTexto[1..2000]}` → `ResultadoEvaluacionDto{esCorrecta, mensaje, nuevoNivel}`  
  Errores: `422 {error:{code:"VALIDATION_ERROR", message, details}}`, `404 {error:{code:"NOT_FOUND"}}`, headers `X-Request-Id`, `X-Content-Type-Options:nosniff`.

## Contributing
- Spec primero (`spec-driven-development`), TDD rojo-verde-refactor, commits atómicos `feat/fix/refactor: ...`, 5 ejes review, `AsNoTracking()` en lecturas, sin MediatR/AutoMapper.
