# Spec: Refactorización Arquitectura EnglishLearning - Cumplimiento AGENTS.md

## Objetivo
Corregir violaciones de desacoplamiento y defectos bloqueantes para alinear la solución con `AGENTS.md:2` (capas estrictas) y `AGENTS.md:35` (simplicidad), sin cambiar comportamiento funcional. Habilitar base limpia para implementar Placement Test y 4 pilares (Reading/Listening/Writing/Speaking).

## Tech Stack
- .NET 9, EF Core 8.0.0, SQL Server
- Arquitectura: Domain (núcleo) → Application → Infrastructure → Api (Composition Root)
- Sin MediatR, sin AutoMapper, sin CQRS. Mapeo manual `ToDto()`.

## Contratos API (existentes, se corrigen sin romper)
- `GET api/lecciones/nivel/{nivel}` → `ILeccionServicio.ObtenerLeccionesPorNivelAsync(NivelIngles)` → `LeccionDto[]`
- `POST api/lecciones/evaluar` → Body `RespuestaDto{UsuarioId, EjercicioId, RespuestaTexto}` → `ResultadoEvaluacionDto{EsCorrecta, Mensaje, NuevoNivel}`
  - Fix: atributo `[POST]` → `[HttpPost]`

## Estructura Proyecto
```
EnglishLearning.Domain/         → Entidades, Enums, Interfaces (sin dependencias)
  Entities/Entidades.cs
  Enums/Enums.cs
  Interfaces/IInterfaces.cs
EnglishLearning.Application/    → Servicios, Dtos, Mappings (ref Domain)
  Services/LeccionServicio.cs
  Dtos/Dtos.cs
  Mappings/LeccionMappingExtensions.cs
EnglishLearning.Infrastructure/ → EF Core, Repositorios (ref Domain ONLY)
  Persistence/EnglishLearningDbContext.cs
  Persistence/Configurations/
  Repositories/Repositorios.cs
EnglishLearning.Api/            → Composition Root (ref Application + Infrastructure)
  Controllers/LeccionesController.cs
  Program.cs
```

## Estilo Código
- Mapeo explícito manual (`Leccion.ToDto()`), no reflexión.
- Servicios inyección directa constructor.
- Consultas lectura con `AsNoTracking()`.
- Nombres en español consistentes con dominio existente (`ObtenerPorNivelAsync`, `EvaluarEjercicioAsync`).

## Estrategia Testing
- Sin suite existente. Verificación por `dotnet build` + revisión manual de contratos.
- Post-refactor: tests unitarios para `LeccionServicio.EvaluarEjercicioAsync` (TDD en siguiente fase).

## Boundaries
- Always: mantener comportamiento, DI en `Program.cs`, `AsNoTracking` en lecturas, DI scoped.
- Ask first: cambios de schema, nuevas dependencias, cambios API breaking.
- Never: introducir MediatR/AutoMapper, acoplar Application↔Infrastructure, commitear secretos.

## Criterios Éxito (verificables)
- [ ] `Infrastructure.csproj` referencia solo `Domain` (no `Application`) — `dotnet list reference` lo confirma
- [ ] `Api.csproj` referencia `Application` y `Infrastructure`
- [ ] `dotnet build` pasa 0 errores, 0 warnings críticos
- [ ] `LeccionesController.cs:26` usa `[HttpPost]`
- [ ] Todos los métodos de lectura en `Repositorios.cs` usan `AsNoTracking()`
- [ ] `LeccionServicio.EvaluarEjercicioAsync` valida `RespuestaCorrecta` real (trim + case-insensitive), persiste `RespuestaUsuario`, calcula nivel correctamente sin simular `esCorrecta=true`
- [ ] `ILeccionRepositorio` expone `ObtenerEjercicioPorIdAsync` e implementado
- [ ] Eliminados `Class1.cs` dead code (3 archivos)
- [ ] `NivelIngles` alineado a A1..C1 (AGENTS.md:12) o justificado si se mantiene C2
- [ ] No se introduce sobreingeniería; no se toca frontend en este slice

## Casos Borde
- Ejercicio inexistente → `Resultado` con `EsCorrecta=false`, mensaje "Ejercicio no encontrado", no persiste ni sube nivel.
- Respuesta vacía/null → validación y `EsCorrecta=false`.
- Usuario inexistente en level-up → no crash, solo no sube nivel.
- Usuario ya en C1 (nivel máximo) → no intenta subir a C2.
- Comparación respuesta: trim + `OrdinalIgnoreCase`.

## Open Questions
- ¿Mantener C2? Spec dice A1-C1, pero CEFR incluye C2. Decisión: ajustar a A1-C1 para cumplir negocio; si Product quiere C2, re-añadir.
