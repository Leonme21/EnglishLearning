# ADR-003: Frontend Angular SPA standalone + Signals

## Status
Accepted — 2026-09-23 — Pendiente implementación (Phase 3)

## Context
`AGENTS.md:43` exige reemplazo total de `EnglishLearning.Client` por SPA Angular última estable, standalone, Signals, consumo REST estricto vía DTOs TS, estructura `features/assessment,reading,listening,speaking,writing,core/services,shared/components`. Actual `Client/src` vacío, sin build Angular, viola desacoplamiento total y bloquea onboarding Placement Test.

## Decision
- Nuevo proyecto Angular 17+ standalone, Vite, `provideRouter`, `Signals` para estado local, `HttpClient` con `withFetch()`.
- DTOs TS tipados espejo de `LeccionDto/ResultadoEvaluacionDto` sin compartir código .NET; contrato `GET /api/lecciones/nivel/{nivel}`, `POST /api/lecciones/evaluar`, futuro `POST /api/assessment/submit`.
- Feature folders `features/assessment` (Placement Test obligatorio), `reading`, `listening`, `writing`, `speaking`; `core/services/api.service.ts`, `core/guards/placement.guard.ts` que bloquea acceso si `usuario.nivelActual==null`.
- CORS `AngularPolicy` en `Api/Program.cs:26` origen `http://localhost:4200` configurable por `appsettings.json`.

## Alternatives Considered
### Mantener Client actual (razor/blazor)
- Pros: ya existe
- Cons: viola `AGENTS.md:43` desacoplamiento total; comparte modelos .NET; no Signals.
- Rechazada.

### React/Next
- Pros: ecosistema amplio
- Cons: AGENTS estipula Angular; divergir requiere ADR supersede.
- Rechazada.

## Consequences
- Backend permanece agnóstico de UI; contrato OpenAPI (`MapOpenApi`) genera tipos.
- Accesibilidad WCAG AA exigida por `frontend-ui-engineering:176`; responsive 320/768/1024/1440, axe-core 0 violaciones.
- Migración: eliminar `EnglishLearning.Client` heredado, crear `english-learning-web/` separado o reemplazar `Client` con `ng new`.
