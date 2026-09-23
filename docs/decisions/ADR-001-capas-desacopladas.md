# ADR-001: Desacoplamiento estricto de capas .NET

## Status
Accepted — 2026-09-23

## Context
`AGENTS.md:28` exige 4 proyectos desacoplados. Se detectó violación: `Infrastructure → Application` (`EnglishLearning.Infrastructure.csproj:4`) y `Api` solo referenciaba `Infrastructure`, rompiendo Composition Root. Esto permitía fugas de DTOs a infraestructura y acoplaba `Application ↔ Infrastructure`, bloqueando testabilidad con Fakes y violando Dependency Inversion.

Requisitos: mantener `Domain` puro (sin deps), `Application` orquesta via interfaces de `Domain`, `Infrastructure` implementa EF Core con `AsNoTracking()`, `Api` compone DI.

## Decision
- `Domain` sin referencias.
- `Application → Domain` únicamente (`EnglishLearning.Application.csproj:4`).
- `Infrastructure → Domain` únicamente (`EnglishLearning.Infrastructure.csproj:4`).
- `Api → Application + Infrastructure` (`EnglishLearning.Api.csproj:14-15`) como único Composition Root (`Program.cs:15-23`).
- Regla verificable: `dotnet list reference` debe mostrar exactamente ese grafo; `Application` e `Infrastructure` jamás se referencian entre sí.

## Alternatives Considered
### Monolito API+Infra juntos
- Pros: menos proyectos
- Cons: imposible sustituir persistencia por Fakes (tests); viola Clean Architecture; acopla EF Core a casos de uso.
- Rechazada: impide TDD con Fakes (`LeccionServicioTests`).

### Shared Kernel con entidades compartidas
- Pros: reutilización
- Cons: crea dependencia bidireccional; `Infrastructure` filtra detalles de mapeo a `Application`.
- Rechazada: mapea `LeccionDto` manual en `Application` evita fuga.

## Consequences
- `LeccionRepositorio`/`UsuarioRepositorio` implementan `ILeccionRepositorio`/`IUsuarioRepositorio` de `Domain` sin conocer `Application`.
- Tests usan `FakeRepositorios.cs` sin EF.
- Riesgo: alguien añade referencia `Infrastructure→Application` para "reutilizar DTOs" — bloqueado por review checklist `code-review-and-quality:52` (Architecture axis).
