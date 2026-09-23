# ADR-004: OpenAPI nativo .NET 9 vs Swashbuckle

## Status
Accepted — 2026-09-23

## Context
`EnglishLearning.Api.csproj:11` referenciaba `Microsoft.AspNetCore.OpenApi 9.0.16` pero `Program.cs:12` llamaba `AddSwaggerGen()`/`UseSwagger()` (Swashbuckle), error CS1061 en build Release. `api-and-interface-design:37` exige contrato-first; `security-and-hardening:53` exige audit lockfile. Añadir Swashbuckle añade 72KB + scripts, dos specs divergentes.

## Decision
- Usar stack nativo .NET 9: `builder.Services.AddOpenApi()` y `app.MapOpenApi()` en Development (`Api/Program.cs:12,40`), mantiene `AddEndpointsApiExplorer()`.
- No añadir Swashbuckle; endpoint `/openapi/v1.json` sirve spec para generar DTOs Angular sin dependencia extra.
- Si se requiere UI Swagger, añadir `Scalar.AspNetCore` o `Swashbuckle` en ADR supersede con justificación.

## Alternatives Considered
### Añadir Swashbuckle.AspNetCore 6.5
- Pros: UI conocida
- Cons: lockfile extra, scripts, divergencia OpenApi; `security-and-hardening:136` obliga auditar scripts.
- Rechazada por KISS; reintroducible si UX exige UI.

## Consequences
- Build verde Debug/Release sin CS1061.
- Contrato versionado vía `/openapi/v1.json`; frontend genera tipos sin `Api` recompilar.
