# Tasks: EnglishLearning — AGENTS.md Strict Compliance

## Task 1: Corregir referencias csproj y dead code
**Description:** Alinear capas a AGENTS.md:28-32. Infrastructure debe ref solo Domain; Api ref Application+Infrastructure.
**Acceptance criteria:**
- [x] `Infrastructure.csproj` referencia solo `Domain`
- [x] `Api.csproj` referencia `Application` y `Infrastructure`
- [x] Eliminados `Domain/Class1.cs`, `Application/Class1.cs`, `Infrastructure/Class1.cs`
**Verification:**
- [x] `dotnet list reference` capas correctas
- [x] `dotnet build` 0 errores
**Dependencies:** None
**Files likely touched:**
- `EnglishLearning.Infrastructure/EnglishLearning.Infrastructure.csproj`
- `EnglishLearning.Api/EnglishLearning.Api.csproj`
**Estimated scope:** S (2 files)

## Task 2: Corregir LeccionServicio evaluación real
**Description:** Eliminar simulación `esCorrecta=true`, implementar validación contra Ejercicio.RespuestaCorrecta, level-up <C1, AsNoTracking.
**Acceptance criteria:**
- [x] `ILeccionRepositorio.ObtenerEjercicioPorIdAsync` existe e implementado con AsNoTracking
- [x] `UsuarioRepositorio.ObtenerPorIdAsync` usa AsNoTracking
- [x] `LeccionServicio.EvaluarEjercicioAsync` valida trim+OrdinalIgnoreCase, maneja null/whitespace, ejercicio no encontrado, persistencia y level-up 80%/10
- [x] `NivelIngles` = A1..C1 con valores 1..5
**Verification:**
- [x] Build verde
- [ ] Tests unitarios (Task 5)
**Dependencies:** Task 1
**Files likely touched:**
- `EnglishLearning.Domain/Enums/Enums.cs`
- `EnglishLearning.Domain/Interfaces/IInterfaces.cs`
- `EnglishLearning.Infrastructure/Repositories/Repositorios.cs`
- `EnglishLearning.Application/Services/LeccionServicio.cs`
**Estimated scope:** M (3-5 files)

## Task 3: Fix Controller y Program.cs OpenApi
**Description:** Corregir atributo POST y alinear Swagger a Microsoft.AspNetCore.OpenApi net9.
**Acceptance criteria:**
- [x] `LeccionesController.cs:26` usa `[HttpPost("evaluar")]`
- [x] `Program.cs` usa `AddOpenApi()`/`MapOpenApi()` (no AddSwaggerGen)
- [x] CORS AngularPolicy restringido a http://localhost:4200, AllowAnyHeader/Method
**Verification:**
- [x] `dotnet build -c Release` verde
**Dependencies:** Task 2
**Files likely touched:**
- `EnglishLearning.Api/Controllers/LeccionesController.cs`
- `EnglishLearning.Api/Program.cs`
**Estimated scope:** S (2 files)

## Task 4: SPEC y build verde
**Description:** Spec 6 áreas (spec-driven-development) para refactor.
**Acceptance criteria:**
- [x] `SPEC-refactor-arquitectura.md` con Objective, Tech Stack, Commands, Project Structure, Code Style, Testing Strategy, Boundaries, Success Criteria, Edge Cases
**Verification:**
- [x] Spec cubre 6 áreas y casos borde
**Dependencies:** Task 3
**Files likely touched:**
- `SPEC-refactor-arquitectura.md`
**Estimated scope:** XS (1 file)

## Checkpoint: Foundation
- [x] Build Debug/Release 0 errores
- [x] Capas verificadas
- [x] Contratos API validados

## Task 5: Test project xUnit + tests LeccionServicio (TDD)
**Description:** Crear proyecto tests y probar lógica dominio/application con red-green-refactor. Cubrir Arrange-Act-Assert, DAMP, real fakes > mocks.
**Acceptance criteria:**
- [ ] `EnglishLearning.Tests` (xUnit) referencia Application+Domain, usa Fakes InMemory para ILeccionRepositorio/IUsuarioRepositorio
- [ ] Tests: ejercicio no encontrado, respuesta correcta trim/case-insensitive, whitespace, nivel sube con 80%/10, no sube si <10 o <80% o ya C1, persiste RespuestaUsuario
- [ ] `dotnet test --coverage` o `dotnet test` verde
**Verification:**
- [ ] `dotnet test` pasa, nombres descriptivos, no skips
- [ ] Full suite verde después de cada test
**Dependencies:** Task 4
**Files likely touched:**
- `EnglishLearning.Tests/EnglishLearning.Tests.csproj`
- `EnglishLearning.Tests/Services/LeccionServicioTests.cs`
- `EnglishLearning.Tests/Fakes/FakeRepositorios.cs`
**Estimated scope:** M (3-5 files)

## Task 6: Validación frontera API + error envelope
**Description:** api-and-interface-design + security-and-hardening: validar input en boundary, error consistente, sin exponer stack trace, queries parametrizadas ya via EF.
**Acceptance criteria:**
- [ ] `RespuestaDto` con DataAnnotations (UsuarioId>0, EjercicioId>0, RespuestaTexto required max 2000) + validación en controller 422 con envelope `{error:{code,message,details}}`
- [ ] `GET nivel/{nivel}` valida enum, paginación opcional `?page&pageSize` (default 1/20)
- [ ] No `innerHTML`/`eval`, headers HSTS/CSP vía middleware
**Verification:**
- [ ] Manual `curl` 422 para payload inválido con envelope consistente
- [ ] `dotnet build` + `dotnet test` verdes
**Dependencies:** Task 5
**Files likely touched:**
- `EnglishLearning.Application/Dtos/Dtos.cs`
- `EnglishLearning.Api/Controllers/LeccionesController.cs`
- `EnglishLearning.Api/Program.cs` (headers)
**Estimated scope:** M (3 files)

## Task 7: ADRs + README + limpieza
**Description:** documentation-and-adrs: ADRs para decisiones reversibles caras.
**Acceptance criteria:**
- [ ] `docs/decisions/ADR-001`..`ADR-004` con Status/Date/Context/Decision/Alternatives/Consequences
- [ ] `README.md` con Quick Start, Commands, Architecture, link ADRs
- [ ] Sin comentarios `what` ni código comentado, inline gotchas documentados
**Verification:**
- [ ] `README.md` permite `dotnet restore && dotnet build && dotnet run`
**Dependencies:** Task 6
**Files likely touched:**
- `docs/decisions/ADR-*.md`
- `README.md`
**Estimated scope:** M (5 files)

## Task 8: Observabilidad mínima + git higiene
**Description:** ILogger structured, correlationId, .gitignore, commits atómicos.
**Acceptance criteria:**
- [ ] `ILogger` en `LeccionServicio`/`LeccionesController` con event `evaluacion_realizada` {usuarioId, ejercicioId, esCorrecta}
- [ ] Middleware correlationId `X-Request-Id`
- [ ] `.gitignore` (bin/obj, .vs, node_modules, dist, .env) y `git init` si no existe
- [ ] Commits atómicos por task con mensajes feat/fix/refactor
**Verification:**
- [ ] `git log --oneline` limpio, `git diff --staged` sin secrets
**Dependencies:** Task 7
**Files likely touched:**
- `EnglishLearning.Api/Program.cs`
- `.gitignore`
**Estimated scope:** S (2-3 files)

## Checkpoint: Quality
- [ ] `dotnet test` verde
- [ ] 5 ejes code-review pasados
- [ ] Security checklist sin críticos

## Task 9: Dominio Placement Test
**Description:** Entidades `ExamenDiagnostico`, `PreguntaDiagnostica` (Reading/Listening/Writing/Speaking, falsos amigos), cálculo Nivel A1-C1.
**Acceptance criteria:**
- [ ] Entidades + ValueObject `PuntajePorHabilidad`, enum `Habilidad` {Reading,Listening,Writing,Speaking}
- [ ] Servicio dominio `CalculadoraNivel` con umbrales testeables
**Verification:**
- [ ] Tests dominio TDD
**Dependencies:** Task 8
**Files likely touched:**
- `EnglishLearning.Domain/Entities/Examen.cs`
- `EnglishLearning.Domain/Services/CalculadoraNivel.cs`
**Estimated scope:** M

## Task 10-12: Application/Angular/Reporte
- [ ] Task 10: AssessmentService + endpoint `POST /api/assessment/submit` → ver tasks/plan.md Phase 3
- [ ] Task 11: Angular SPA standalone Signals features/assessment
- [ ] Task 12: Métricas por habilidad + recomendaciones

## Checkpoint: Complete
- [ ] Onboarding obligatorio guarda bloquea sin nivel
- [ ] Responsive 320/768/1024/1440, axe-core 0 violaciones
- [ ] Reporte funcional con ideas hispanohablantes
