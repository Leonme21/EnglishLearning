# Spec: Placement Test Obligatorio + 4 Pilares + Reporte

## Objetivo
Cumplir `AGENTS.md:11-22`: onboarding bloqueante (usuario sin Nivel no accede a lecciones), examen diagnóstico adapta A1-C1, módulos por habilidad (Reading/Listening/Writing/Speaking) con ejercicios hispanohablante-focus, y reporte métricas por habilidad + recomendaciones.

## Contratos API (contract-first)

### Assessment (Placement)
- `GET /api/assessment/preguntas` → `PreguntaDiagnosticaDto[]{id, habilidad, contenido, opciones[], nivel}` — 12-20 preguntas balanceadas 3 por habilidad, incluye falsos amigos *actually/embarrassed/constipated/currently*.
  - Auth: no, onboarding. Rate-limit 10/15min (security).
  - 200 + `X-Request-Id`
- `POST /api/assessment/submit` → Body `SubmitAssessmentDto{usuarioId, respuestas:[{preguntaId, respuestaTexto}]}` → `ResultadoAssessmentDto{nivelAsignado:"B1", puntajeTotal:65, puntajesPorHabilidad:{reading:70,listening:50,writing:80,speaking:60}, mensaje, siguienteRecomendacion}` 
  - Validación frontera: usuarioId>0, 8-20 respuestas, cada respuesta 1..2000 chars, preguntaId debe existir.
  - Casos: usuario no existe 404, <8 respuestas 422, preguntaId inválido 422, ya evaluado? idempotente (re-evalúa y actualiza nivel).
  - Efecto: `Usuario.NivelActual = nivelAsignado`, crea `ProgresoNivel` por habilidad, persiste `RespuestaUsuario` marcadas EsCorrecta.
  - Cálculo nivel (CalculadoraNivel dominio): porcentaje correctas 0-100 → A1 0-25, A2 26-45, B1 46-65, B2 66-85, C1 86-100. Puro dominio, testeable.

### Lecciones (existente, respeta guarda)
- `GET /api/lecciones/nivel/{nivel}` — ahora requiere usuario evaluado; si `Usuario.NivelActual==SinAsignar` → 403 `{error:{code:"PLACEMENT_REQUIRED"}}`. Paginación `?page=1&pageSize=20`.
- `POST /api/lecciones/evaluar` — ya 422 envelope, persiste y puede subir nivel.

### Reporte
- `GET /api/progreso/{usuarioId}` → `ReporteProgresoDto{usuarioId, nivelActual, progresos:[{nivel,porcentajeCompletado}], metricasPorHabilidad:{reading:{aciertos,total,porcentaje},...}, recomendaciones: string[]}`
  - 404 usuario no existe, 403 si SinAsignar (sin datos).

## DTOs TypeScript (Angular)
```ts
interface PreguntaDiagnosticaDto { id:number; habilidad:'Reading'|'Listening'|'Writing'|'Speaking'; contenido:string; opciones:string[]; nivel:string; }
interface SubmitAssessmentDto { usuarioId:number; respuestas:{preguntaId:number; respuestaTexto:string}[]; }
interface ResultadoAssessmentDto { nivelAsignado:string; puntajeTotal:number; puntajesPorHabilidad:Record<Habilidad,number>; mensaje:string; siguienteRecomendacion:string; }
```

## Casos Borde
- Usuario nuevo `NivelActual=SinAsignar (0)` → GET lecciones 403, POST submit ok.
- Submit con respuesta vacía/whitespace → cuenta como incorrecta (no 422 si campo presente, pero trim vacía = incorrecta).
- Submit con preguntaId duplicado → 422.
- Empate en umbral: 65% → B1, 66% → B2 (límite superior inclusive nivel inferior).
- 0% → A1, 100% → C1.
- Re-evalúa: actualiza nivel, no duplica progresos, sobrescribe métricas.
- Transferencias hispano: pregunta "I am agree" espera "I agree" (validación trim+case-insensitive).
- Empty DB preguntas → GET 200 [] y POST 422 "sin preguntas configuradas".

## Comandos
```
dotnet build
dotnet test --filter Assessment
ng serve --port 4200
```

## Estructura
```
Domain/Entities/Assessment.cs (PreguntaDiagnostica, EvaluacionDiagnostica)
Domain/Enums/Habilidad.cs + NivelIngles.SinAsignar
Domain/Services/CalculadoraNivel.cs (puro, tests unitarios)
Application/Services/AssessmentService.cs (ILeccionServicio-like)
Application/Dtos/AssessmentDtos.cs
Infrastructure/Repositories/AssessmentRepositorio.cs + seed 16 preguntas
Api/Controllers/AssessmentController.cs + ProgresoController.cs
Angular: features/assessment/{assessment.component.ts, assessment.service.ts}, core/guards/placement.guard.ts, core/services/api.service.ts
```

## Criterios Éxito
- [ ] Usuario SinAsignar bloqueado 403 en lecciones
- [ ] Submit 16 preguntas → nivel correcto por umbrales, persiste nivel y métricas
- [ ] Tests dominio CalculadoraNivel (umbrales), AssessmentService (guardar, nivel, métricas) verdes
- [ ] Angular assessment standalone Signals, guarda placement, DTOs tipados
- [ ] Reporte por habilidad con recomendaciones hispanohablante
- [ ] Build+tests verde, OpenApi expone /assessment

## Open Questions
- ¿12 o 16 preguntas diagnóstico? Elegido 16 (4 por habilidad) para % granular.
