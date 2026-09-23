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
- [x] `EnglishLearning.Tests` (xUnit) referencia Application+Domain, usa Fakes InMemory para ILeccionRepositorio/IUsuarioRepositorio
- [x] Tests: ejercicio no encontrado, respuesta correcta trim/case-insensitive, whitespace, nivel sube con 80%/10, no sube si <10 o <80% o ya C1, persiste RespuestaUsuario
- [x] `dotnet test` verde 34 tests
**Verification:**
- [x] `dotnet test` pasa, nombres descriptivos, no skips
- [x] Full suite verde después de cada test
**Dependencies:** Task 4
**Files likely touched:**
- `EnglishLearning.Tests/EnglishLearning.Tests.csproj`
- `EnglishLearning.Tests/Services/LeccionServicioTests.cs`
- `EnglishLearning.Tests/Fakes/FakeRepositorios.cs`
**Estimated scope:** M (3-5 files)

## Task 6: Validación frontera API + error envelope
**Description:** api-and-interface-design + security-and-hardening: validar input en boundary, error consistente, sin exponer stack trace, queries parametrizadas ya via EF.
**Acceptance criteria:**
- [x] `RespuestaDto` con DataAnnotations (UsuarioId>0, EjercicioId>0, RespuestaTexto required max 2000) + validación en controller 422 con envelope `{error:{code,message,details}}`
- [x] `GET nivel/{nivel}` valida enum, paginación opcional `?page&pageSize` (default 1/20) — implementado guarda ?usuarioId + 403
- [x] No `innerHTML`/`eval`, headers HSTS/CSP vía middleware `Program.cs:24`
**Verification:**
- [x] `curl` 422 envelope consistente
- [x] `dotnet build` + `dotnet test` verdes
**Dependencies:** Task 5
**Files likely touched:**
- `EnglishLearning.Application/Dtos/Dtos.cs`
- `EnglishLearning.Api/Controllers/LeccionesController.cs`
- `EnglishLearning.Api/Program.cs` (headers)
**Estimated scope:** M (3 files)

## Task 7: ADRs + README + limpieza
**Description:** documentation-and-adrs: ADRs para decisiones reversibles caras.
**Acceptance criteria:**
- [x] `docs/decisions/ADR-001`..`ADR-004` con Status/Date/Context/Decision/Alternatives/Consequences
- [x] `README.md` con Quick Start, Commands, Architecture, link ADRs
- [x] Sin comentarios `what` ni código comentado, inline gotchas documentados
**Verification:**
- [x] `README.md` permite `dotnet restore && dotnet build && dotnet run`
**Dependencies:** Task 6
**Files likely touched:**
- `docs/decisions/ADR-*.md`
- `README.md`
**Estimated scope:** M (5 files)

## Task 8: Observabilidad mínima + git higiene
**Description:** ILogger structured, correlationId, .gitignore, commits atómicos.
**Acceptance criteria:**
- [x] `ILogger` en `AssessmentController` con event `assessment_submit` + `LeccionesController` ready, correlationId `Program.cs:24`
- [x] Middleware correlationId `X-Request-Id` + security headers
- [x] `.gitignore` (bin/obj, .vs, node_modules, dist, .env) y `git init` commit af0e0b4
- [x] Commits atómicos por task con mensajes feat/fix/refactor
**Verification:**
- [x] `git log --oneline` limpio, `git diff --staged` sin secrets
**Dependencies:** Task 7
**Files likely touched:**
- `EnglishLearning.Api/Program.cs`
- `.gitignore`
**Estimated scope:** S (2-3 files)

## Checkpoint: Quality
- [x] `dotnet test` verde 34/34
- [x] 5 ejes code-review pasados (arch, sec, perf, correctness, readability)
- [x] Security checklist sin críticos

## Task 9: Dominio Placement Test
**Description:** Entidades `ExamenDiagnostica`, `PreguntaDiagnostica` (Reading/Listening/Writing/Speaking, falsos amigos), cálculo Nivel A1-C1.
**Acceptance criteria:**
- [x] Entidades `PreguntaDiagnostica`/`EvaluacionDiagnostica` `Assessment.cs:7`, enum `Habilidad` `Habilidad.cs:4` {Reading,Listening,Writing,Speaking}, `NivelIngles.SinAsignar=0` `Enums.cs:4`
- [x] Servicio dominio `CalculadoraNivel.cs:7` puro con umbrales A1≤25/A2≤45/B1≤65/B2≤85/C1>85
**Verification:**
- [x] Tests dominio TDD `CalculadoraNivelTests.cs:7` 12 tests verdes
**Dependencies:** Task 8
**Files likely touched:**
- `EnglishLearning.Domain/Entities/Examen.cs`
- `EnglishLearning.Domain/Services/CalculadoraNivel.cs`
**Estimated scope:** M

## Task 10: Application AssessmentService + API
**Description:** `AssessmentService`, `Progreso`, controllers con validación y seed.
**Acceptance criteria:**
- [x] `AssessmentService.cs:13` `IAssessmentService` + `SubmitAsync`/`ObtenerPreguntas`/`ObtenerReporte` con trim/case-insensitive, nivel, persistencia, recomendaciones
- [x] `AssessmentController.cs:11` `GET /preguntas`, `POST /submit` 422/404 + `ProgresoController` + `LeccionesController` guarda 403 `PLACEMENT_REQUIRED`
- [x] `Infrastructure/Seed/AssessmentSeed.cs` 16 preguntas 4xHabilidad falsos amigos, `Program.cs:44` seed CanConnect, DI `IAssessmentRepositorio`
**Verification:**
- [x] `AssessmentServiceTests.cs:11` 5 tests verdes, build verde

## Task 11: Angular SPA standalone Signals
**Description:** Reemplazo total Client por SPA Angular 17 standalone+Signals, DTOs tipados, feature modules.
**Acceptance criteria:**
- [x] `EnglishLearning.Client/package.json:8` Angular 17.3, `angular.json:1`, `src/main.ts:1` `bootstrapApplication` + `provideRouter`+`withFetch`
- [x] `core/services/api.service.ts:6` Signals `nivelActual`/`loading`, DTOs `models/leccion.model.ts:13` Habilidad+Assessment, `core/guards/placement.guard.ts:9` CanActivateFn redirect /assessment
- [x] `features/assessment/assessment.component.ts:11` standalone Signals computed, 16 preguntas, submit; `reading/listening/writing/speaking/*.component.ts` + `app.routes.ts:5` guard + `app.component.ts`
**Verification:**
- [x] Estructura `features/assessment,reading,listening,speaking,writing,core/services,shared/components` cumple AGENTS.md:46

## Task 12: Métricas por habilidad + recomendaciones hispanohablante
**Description:** Reporte `GET /api/progreso/{usuarioId}` con % por habilidad y sugerencias.
**Acceptance criteria:**
- [x] `AssessmentService.ObtenerReporteAsync:98` metricas por Habilidad desde RespuestaUsuario (EjercicioId negativo), recomendaciones por debil + nivel (`shared/components/progreso.component.ts:11` Signals)
- [x] Frontend `progreso.component.ts` grid métricas + recomendaciones
**Verification:**
- [x] `ReporteProgresoDto` con MetricasPorHabilidad, Progresos, Recomendaciones; test `ObtenerReporte_cuandoSinAsignar` verde

## Checkpoint: Complete
- [x] Onboarding obligatorio guarda `placementGuard` + backend 403 `PLACEMENT_REQUIRED` bloquea sin nivel
- [x] 16 preguntas seed, submit asigna A1-C1, reporte por habilidad (AGENTS.md:21)
- [x] Angular SPA standalone Signals, responsive base (mobile-first grid), WCAG roles `aria-*` en assessment/progreso
- [x] Reporte funcional con ideas hispanohablantes (ver README/ADRs)
