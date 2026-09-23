# Implementation Plan: Refactorización Arquitectura + Base Placement Test

## Overview
Alinear solución con `AGENTS.md` (capas estrictas, KISS, SPA Angular) mediante refactorización backend ya ejecutada (Slices 1-2) y preparar base para onboarding obligatorio Placement Test (A1-C1) y 4 pilares. Plan vertical: cada slice deja sistema compilable y testeable.

## Architecture Decisions
- **ADR-001**: Capas desacopladas `Domain → Application → Infrastructure → Api` (Api único Composition Root) — ver `docs/decisions/ADR-001-capas-desacopladas.md`.
- **ADR-002**: Sin MediatR/AutoMapper/CQRS; mapeo manual `ToDto()`, inyección directa `LeccionServicio`, `AsNoTracking()` en lecturas.
- **ADR-003**: Angular SPA standalone+Signals, DTOs tipados, `features/assessment,reading,listening,speaking,writing` — reemplazo total `EnglishLearning.Client`.
- **ADR-004**: `Microsoft.AspNetCore.OpenApi` (net9) vía `AddOpenApi()/MapOpenApi()` en vez de Swashbuckle; mantiene 1 dependencia menos.

## Dependency Graph
```
Domain (Entidades, Enums, Interfaces)
  └── Application (Dtos, Mappings, Services)
        └── Infrastructure (EF Core, Repos) — ref Domain ONLY
              └── Api (Controllers, DI, OpenApi, CORS) — ref Application+Infrastructure
                    └── Angular SPA (DTOs TS, features/*)
```

## Task List (vertical slices)

### Phase 1: Foundation — Refactorización Backend (COMPLETADO)
- [x] Task 1: Corregir referencias csproj y dead code
- [x] Task 2: Corregir LeccionServicio evaluación real + Repositorio EjercicioPorId + AsNoTracking + NivelIngles A1-C1
- [x] Task 3: Fix LeccionesController [HttpPost] y Program.cs OpenApi
- [x] Task 4: SPEC + build verde Debug/Release

### Checkpoint: Foundation
- [x] `dotnet build` 0 errores
- [x] `dotnet list reference` capas correctas
- [x] Contratos API validados (GET nivel/{nivel}, POST evaluar)

### Phase 2: Quality Gates (EN CURSO — este plan)
- [ ] Task 5: Test project xUnit + tests LeccionServicio (TDD, 80% lógica dominio)
- [ ] Task 6: Validación en frontera API (api-and-interface-design, security) + error envelope consistente
- [ ] Task 7: ADR-001..004 + README + limpieza comentarios/what
- [ ] Task 8: Observabilidad mínima (ILogger structured + correlationId) + .gitignore + git atomic commits

### Checkpoint: Quality
- [ ] `dotnet test` verde
- [ ] Code-review 5 ejes pasado
- [ ] No secrets, queries parametrizadas, CORS explícito

### Phase 3: Placement Test Vertical Slice (SIGUIENTE)
- [ ] Task 9: Dominio `ExamenDiagnostico`, `Pregunta`, `Intento` + cálculo Nivel A1-C1 (regla negocio AGENTS.md:11)
- [ ] Task 10: Application `AssessmentService` + Dtos + endpoint `POST /api/assessment/submit`
- [ ] Task 11: Angular `features/assessment` (standalone, Signals, DTOs tipados) + guarda `placementGuard`
- [ ] Task 12: Reporte métricas por habilidad (Reading/Listening/Writing/Speaking)

### Checkpoint: Complete
- [ ] Onboarding obligatorio bloquea acceso sin nivel
- [ ] Métricas por habilidad visibles
- [ ] E2E `dotnet test` + manual Angular 320/768/1024/1440

## Risks and Mitigations
| Risk | Impact | Mitigación |
|------|--------|------------|
| Frontend Client vacío, no Angular | Alto | Task 11 dedicado, contrato DTOs primero (contract-first) |
| Sin tests actuales | Alto | Task 5 crea suite xUnit antes de nuevo dominio (TDD) |
| EF Core 8 en net9 | Medio | Pin 8.0.0 probado; migrar a 9 solo con changelog review |
| CORS hardcode localhost:4200 | Medio | Configurable vía appsettings + validación origen |
| No git repo (code-review higiene) | Medio | Init git, .gitignore, commits atómicos por slice |

## Open Questions
- ¿Mantener C2? Decidido A1-C1 por AGENTS.md:12; re-añadir solo si Product lo exige (nuevo ADR).
- ¿Paginación en GET lecciones? `api-and-interface-design` pide paginación; añadir en Task 6 o 9.

## Parallelization
- Seguro paralelo: Task 5 (tests) y Task 7 (docs) tras Task 6.
- Secuencial: Task 9→10→11 (dominio→api→ui).
- Coordinación: contrato `AssessmentDto` definido en Task 9 antes de 10/11.

## Tasks tracked in
`tasks/todo.md` (default markdown). No external tracker designado por `AGENTS.md`.
