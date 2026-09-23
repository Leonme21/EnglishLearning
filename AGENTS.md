# Protocolo de Agente y Directivas Técnicas - EnglishLearning

Este archivo rige el comportamiento de cualquier agente de codificación en esta solución. Las directivas de proceso se apoyan en las skills ubicadas en `.opencode/skills/`.

---

## 1. Visión del Producto
Sistema web integral para el aprendizaje de inglés enfocado específicamente en **hispanohablantes** (abordando falsos amigos, vicios de pronunciación y transferencias gramaticales comunes del español al inglés).

### Reglas de Negocio Clave
1. **Onboarding obligatorio (Placement Test):**
   - Todo nuevo usuario debe completar un examen diagnóstico inicial antes de acceder al contenido.
   - El sistema calcula y asigna un nivel del marco común (A1, A2, B1, B2, C1).
2. **Método por Habilidades (4 Pilares):**
   - Una vez asignado el nivel, el usuario accede a módulos adaptados con ejercicios prácticos en:
     - **Reading:** Comprensión de textos con vocabulario contextual.
     - **Listening:** Audios con preguntas de comprensión y transcripción.
     - **Writing:** Redacción con validaciones léxicas y gramaticales.
     - **Speaking:** Práctica de pronunciación y fluidez guiada.
3. **Reporte y Recomendaciones Continuas:**
   - Métricas de desempeño por habilidad.
   - Al finalizar etapas, el agente debe proponer sugerencias técnicas y funcionales para enriquecer la plataforma.

---

## 2. Restricciones y Arquitectura Backend (.NET)

### Desacoplamiento Estricto de Capas (Prohibidas referencias cruzadas)
- **`EnglishLearning.Domain`:** Núcleo puro. Entidades de negocio, Value Objects, enums y contratos/interfaces base (repositorios, puertos de servicios externos). No depende de nadie.
- **`EnglishLearning.Application`:** Casos de uso y orquestación de negocio. Consume las interfaces definidas en `Domain`. **NO referencia a `Infrastructure`**.
- **`EnglishLearning.Infrastructure`:** Implementación técnica (EF Core, proveedores externos, I/O). Implementa las interfaces de `Domain`. **NO referencia a `Application`**.
- **`EnglishLearning.Api`:** Composition Root y capa de exposición REST. Es el **único** proyecto que referencia tanto a `Application` como a `Infrastructure` para resolver la Inyección de Dependencias (DI) en `Program.cs`.
- **Regla estricta:** `Application` e `Infrastructure` jamás se referencian directamente entre sí.

### Simplicidad y Cero Sobreingeniería (Keep It Simple)
- **Sin MediatR:** Utilizar inyección directa de servicios de aplicación o casos de uso (`LessonService`, `AssessmentService`, etc.).
- **Sin AutoMapper ni librerías de mapeo por reflexión:** Implementar mapeo explícito manual mediante extensiones o constructores (`ToDto()`, `ToEntity()`). Facilita depuración, trazabilidad y rendimiento.
- **Sin CQRS complejo:** No crear duplicidad innecesaria de handlers de comandos y queries si una llamada a servicio o repositorio directo resuelve la operación.
- **Consultas eficientes:** Usar `AsNoTracking()` en Entity Framework Core para todas las lecturas.

---

## 3. Arquitectura Frontend (Angular)
- **Desacoplamiento total:** Reemplazar por completo cualquier cliente existente previo por una SPA en **Angular** (última versión estable, standalone components, Angular Signals).
- **Consumo REST estricto:** La aplicación Angular interactúa con el backend únicamente a través de la API REST mediante DTOs tipados en TypeScript (interfaces), sin compartir dependencias ni código con .NET.
- **Estructura modular por feature:** `features/assessment`, `features/reading`, `features/listening`, `features/speaking`, `features/writing`, `core/services`, `shared/components`.

---

## 4. Ciclo de Trabajo Obligatorio para el Agente

Antes de ejecutar tareas, el agente debe guiarse por las skills instaladas:

1. **Definición (`spec-driven-development`):**
   - Para cada nueva funcionalidad o refactor, redactar primero el contrato de API (rutas, verbos, DTOs de entrada/salida) y los casos de borde. Prohibido codificar sin especificación previa aprobada.
2. **Pruebas (`test-driven-development`):**
   - Diseñar pruebas unitarias para las reglas de negocio del Dominio y servicios de Aplicación antes de escribir el código productivo.
3. **Calidad y Revisión (`code-review-and-quality` / `code-simplification`):**
   - Validar que ninguna clase viole los límites entre capas.
   - Eliminar código muerto, sobreingeniería y abstracciones vacías.
4. **Reporte Funcional:**
   - Al concluir una sesión de refactor o entrega de módulo, incluir un bloque con:
     - Estado de la refactorización.
     - Ideas o funcionalidades complementarias recomendadas para el aprendizaje de hispanohablantes.