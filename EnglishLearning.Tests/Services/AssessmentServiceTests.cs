using System.Text.Json;
using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Tests.Fakes;

namespace EnglishLearning.Tests.Services;

public class AssessmentServiceTests
{
    private FakeAssessmentRepositorio CrearAssessmentConPreguntas()
    {
        var repo = new FakeAssessmentRepositorio();
        // 4 preguntas, 1 por habilidad
        repo.Agregar(new PreguntaDiagnostica { Id=1, Habilidad=Habilidad.Reading, Contenido="Actually", OpcionesJson=JsonSerializer.Serialize(new[]{"En realidad"}), RespuestaCorrecta="En realidad", Nivel=NivelIngles.A2 });
        repo.Agregar(new PreguntaDiagnostica { Id=2, Habilidad=Habilidad.Listening, Contenido="think /θ/", OpcionesJson=JsonSerializer.Serialize(new[]{"th"}), RespuestaCorrecta="th voiceless /θ/", Nivel=NivelIngles.A2 });
        repo.Agregar(new PreguntaDiagnostica { Id=3, Habilidad=Habilidad.Writing, Contenido="I am 30", OpcionesJson=JsonSerializer.Serialize(new[]{"I am 30 years old"}), RespuestaCorrecta="I am 30 years old", Nivel=NivelIngles.A1 });
        repo.Agregar(new PreguntaDiagnostica { Id=4, Habilidad=Habilidad.Speaking, Contenido="vegetable 3", OpcionesJson=JsonSerializer.Serialize(new[]{"3"}), RespuestaCorrecta="3", Nivel=NivelIngles.B1 });
        return repo;
    }

    [Fact]
    public async Task Submit_cuandoTodasCorrectas_asignaC1_yPersisteNivel()
    {
        var assessmentRepo = CrearAssessmentConPreguntas();
        // Añadir 4 más para tener 8 totales (necesario >=8 por validación)
        for(int i=5;i<=8;i++) assessmentRepo.Agregar(new PreguntaDiagnostica{Id=i, Habilidad=Habilidad.Reading, Contenido=$"Q{i}", OpcionesJson="[]", RespuestaCorrecta="ok", Nivel=NivelIngles.A1});
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(new Usuario{Id=1, Nombre="U1", Correo="u1@test.com", NivelActual=NivelIngles.SinAsignar});
        var sut = new AssessmentService(assessmentRepo, usuarioRepo);

        var dto = new SubmitAssessmentDto
        {
            UsuarioId=1,
            Respuestas = Enumerable.Range(1,8).Select(id => new RespuestaAssessmentDto{ PreguntaId=id, RespuestaTexto= assessmentRepo.Preguntas[id].RespuestaCorrecta }).ToList()
        };

        var res = await sut.SubmitAsync(dto);

        Assert.Equal("C1", res.NivelAsignado);
        Assert.Equal(100, res.PuntajeTotal);
        var usuario = await usuarioRepo.ObtenerPorIdAsync(1);
        Assert.Equal(NivelIngles.C1, usuario!.NivelActual);
        Assert.Equal(8, usuarioRepo.RespuestasPersistidas.Count);
        Assert.Contains("Perfecciona", res.SiguienteRecomendacion);
    }

    [Fact]
    public async Task Submit_cuandoPreguntaDuplicada_lanzaArgumentException()
    {
        var assessmentRepo = CrearAssessmentConPreguntas();
        for(int i=5;i<=8;i++) assessmentRepo.Agregar(new PreguntaDiagnostica{Id=i, Habilidad=Habilidad.Reading, Contenido=$"Q{i}", OpcionesJson="[]", RespuestaCorrecta="ok", Nivel=NivelIngles.A1});
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(new Usuario{Id=1, Nombre="U", Correo="u@test.com", NivelActual=NivelIngles.SinAsignar});
        var sut = new AssessmentService(assessmentRepo, usuarioRepo);
        var dto = new SubmitAssessmentDto
        {
            UsuarioId=1,
            Respuestas = new List<RespuestaAssessmentDto>
            {
                new(){PreguntaId=1, RespuestaTexto="En realidad"},
                new(){PreguntaId=1, RespuestaTexto="En realidad"},
                new(){PreguntaId=2, RespuestaTexto="th voiceless /θ/"},
                new(){PreguntaId=3, RespuestaTexto="I am 30 years old"},
                new(){PreguntaId=4, RespuestaTexto="3"},
                new(){PreguntaId=5, RespuestaTexto="ok"},
                new(){PreguntaId=6, RespuestaTexto="ok"},
                new(){PreguntaId=7, RespuestaTexto="ok"},
            }
        };
        await Assert.ThrowsAsync<ArgumentException>(() => sut.SubmitAsync(dto));
    }

    [Fact]
    public async Task Submit_cuandoUsuarioNoExiste_lanzaKeyNotFound()
    {
        var assessmentRepo = CrearAssessmentConPreguntas();
        var usuarioRepo = new FakeUsuarioRepositorio();
        var sut = new AssessmentService(assessmentRepo, usuarioRepo);
        var dto = new SubmitAssessmentDto{UsuarioId=99, Respuestas=new List<RespuestaAssessmentDto>{new(){PreguntaId=1, RespuestaTexto="x"}}};
        await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.SubmitAsync(dto));
    }

    [Fact]
    public async Task Submit_trimCaseInsensitive_cuentaComoCorrecta()
    {
        var assessmentRepo = CrearAssessmentConPreguntas();
        for(int i=5;i<=8;i++) assessmentRepo.Agregar(new PreguntaDiagnostica{Id=i, Habilidad=Habilidad.Reading, Contenido=$"Q{i}", OpcionesJson="[]", RespuestaCorrecta="ok", Nivel=NivelIngles.A1});
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(new Usuario{Id=2, Nombre="U2", Correo="u2@test.com", NivelActual=NivelIngles.SinAsignar});
        var sut = new AssessmentService(assessmentRepo, usuarioRepo);
        var dto = new SubmitAssessmentDto
        {
            UsuarioId=2,
            Respuestas = new List<RespuestaAssessmentDto>
            {
                new(){PreguntaId=1, RespuestaTexto="  en REALIDAD  "}, // should be correct
                new(){PreguntaId=2, RespuestaTexto="th voiceless /θ/"},
                new(){PreguntaId=3, RespuestaTexto="wrong"},
                new(){PreguntaId=4, RespuestaTexto="3"},
                new(){PreguntaId=5, RespuestaTexto="ok"},
                new(){PreguntaId=6, RespuestaTexto="ok"},
                new(){PreguntaId=7, RespuestaTexto="ok"},
                new(){PreguntaId=8, RespuestaTexto="ok"},
            }
        };
        var res = await sut.SubmitAsync(dto);
        // 7 correctas de 8 = 87.5% => C1
        Assert.Equal("C1", res.NivelAsignado);
    }

    [Fact]
    public async Task ObtenerReporte_cuandoSinAsignar_recomiendaPlacement()
    {
        var assessmentRepo = CrearAssessmentConPreguntas();
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(new Usuario{Id=10, Nombre="N", Correo="n@test.com", NivelActual=NivelIngles.SinAsignar});
        var sut = new AssessmentService(assessmentRepo, usuarioRepo);
        var rep = await sut.ObtenerReporteAsync(10);
        Assert.Equal("SinAsignar", rep.NivelActual);
        Assert.Contains(rep.Recomendaciones, r => r.Contains("Placement Test"));
        Assert.Equal(4, rep.MetricasPorHabilidad.Count);
    }
}

// Fake extendido para tests Assessment (mantiene simple)
public class FakeAssessmentRepositorio : EnglishLearning.Domain.Interfaces.IAssessmentRepositorio
{
    public Dictionary<int, PreguntaDiagnostica> Preguntas = new();
    public List<EvaluacionDiagnostica> Evaluaciones = new();
    public void Agregar(PreguntaDiagnostica p) => Preguntas[p.Id]=p;
    public Task<IEnumerable<PreguntaDiagnostica>> ObtenerPreguntasAsync() => Task.FromResult<IEnumerable<PreguntaDiagnostica>>(Preguntas.Values.OrderBy(p=>p.Id));
    public Task<PreguntaDiagnostica?> ObtenerPreguntaPorIdAsync(int id) => Task.FromResult(Preguntas.TryGetValue(id, out var p)?p:null);
    public Task GuardarEvaluacionAsync(EvaluacionDiagnostica e) { Evaluaciones.Add(e); return Task.CompletedTask; }
    public Task<EvaluacionDiagnostica?> ObtenerUltimaEvaluacionAsync(int usuarioId) => Task.FromResult(Evaluaciones.Where(e=>e.UsuarioId==usuarioId).OrderByDescending(e=>e.Fecha).FirstOrDefault());
}
