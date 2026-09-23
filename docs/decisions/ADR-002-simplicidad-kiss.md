# ADR-002: Simplicidad — Sin MediatR/AutoMapper/CQRS

## Status
Accepted — 2026-09-23

## Context
`AGENTS.md:35` proscribe sobreingeniería. Código heredado insinuaba MediatR/handlers, AutoMapper por reflexión y CQRS duplicado. Equipo pequeño, 4 pilares (Reading/Listening/Writing/Speaking) con CRUD simple; latencia y trazabilidad importan más que bus de mediación. Mapeo por reflexión rompe `AsNoTracking()` control y dificulta debug hispanohablante (falsos amigos requieren lógica explícita).

## Decision
- Inyección directa `ILeccionServicio`/`LeccionServicio` (`Application/Services/LeccionServicio.cs:15`), sin MediatR.
- Mapeo explícito manual `LeccionExtensions.ToDto()` (`Application/Mappings/LeccionMappingExtensions.cs:9`), sin AutoMapper.
- Sin separación Command/Query; servicio directo + repositorio.
- Consultas lectura siempre `AsNoTracking()` (`Infrastructure/Repositories/Repositorios.cs:21,31,39,57,79`).

## Alternatives Considered
### MediatR + CQRS
- Pros: desacopla handlers
- Cons: +2 clases por operación (Command/Query/Handler), indirección sin beneficio con <10 casos de uso; dificulta `code-simplification:94` (abstracciones no rentables).
- Rechazada hasta 3er caso de uso idéntico.

### AutoMapper
- Pros: menos código mapeo
- Cons: perfil reflexión oculta, rompe rename-refactor, coste perf; `code-review:62` exige boundaries tipados explícitos.
- Rechazada: `ToDto()` 8 líneas trazables.

## Consequences
- Menos dependencias, build 1.2s, debug directo.
- Si aparece flujo complejo (Placement Test con cálculo nivel), reevaluar con nuevo ADR; no refactorizar anticipadamente.
