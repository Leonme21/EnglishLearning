using EnglishLearning.Application.Dtos;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Tests.Fakes;

namespace EnglishLearning.Tests.Services;

public class LeccionServicioTests
{
    private static Ejercicio CrearEjercicio(int id, string respuestaCorrecta) => new()
    {
        Id = id,
        LeccionId = 1,
        Contenido = "Translate: actually",
        RespuestaCorrecta = respuestaCorrecta,
        Tipo = TipoEjercicio.Escritura
    };

    private static Usuario CrearUsuario(int id, NivelIngles nivel) => new()
    {
        Id = id,
        Nombre = "Test User",
        Correo = "test@example.com",
        NivelActual = nivel
    };

    [Fact]
    public async Task Evaluar_cuandoEjercicioNoExiste_retornaNoCorrecta_yMensajeNoEncontrado()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        var usuarioRepo = new FakeUsuarioRepositorio();
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var dto = new RespuestaDto { UsuarioId = 1, EjercicioId = 99, RespuestaTexto = "hello" };

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(dto);

        // Assert
        Assert.False(resultado.EsCorrecta);
        Assert.Equal("Ejercicio no encontrado.", resultado.Mensaje);
        Assert.Null(resultado.NuevoNivel);
        Assert.Empty(usuarioRepo.RespuestasPersistidas);
    }

    [Fact]
    public async Task Evaluar_cuandoRespuestaCorrectaExacta_retornaEsCorrectaTrue()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(1, "actually means en realidad"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.A1));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var dto = new RespuestaDto { UsuarioId = 1, EjercicioId = 1, RespuestaTexto = "actually means en realidad" };

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(dto);

        // Assert
        Assert.True(resultado.EsCorrecta);
        Assert.Equal("¡Excelente trabajo!", resultado.Mensaje);
        Assert.Null(resultado.NuevoNivel);
        Assert.Single(usuarioRepo.RespuestasPersistidas);
        Assert.True(usuarioRepo.RespuestasPersistidas[0].EsCorrecta);
    }

    [Fact]
    public async Task Evaluar_cuandoRespuestaConEspaciosYMayusculas_retornaEsCorrectaTrue()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(10, "I am hungry"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.A2));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var dto = new RespuestaDto { UsuarioId = 1, EjercicioId = 10, RespuestaTexto = "  i AM hungry  " };

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(dto);

        // Assert
        Assert.True(resultado.EsCorrecta);
    }

    [Fact]
    public async Task Evaluar_cuandoRespuestaVacia_retornaEsCorrectaFalse()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(2, "hello"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.B1));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);

        // Act
        var r1 = await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 1, EjercicioId = 2, RespuestaTexto = "" });
        var r2 = await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 1, EjercicioId = 2, RespuestaTexto = "   " });
        var r3 = await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 1, EjercicioId = 2, RespuestaTexto = null! });

        // Assert
        Assert.False(r1.EsCorrecta);
        Assert.False(r2.EsCorrecta);
        Assert.False(r3.EsCorrecta);
        Assert.All(new[] { r1, r2, r3 }, r => Assert.Equal("Sigue practicando.", r.Mensaje));
    }

    [Fact]
    public async Task Evaluar_cuandoRespuestaIncorrecta_retornaMensajeSiguePracticando()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(3, "correct"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(5, NivelIngles.A1));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var dto = new RespuestaDto { UsuarioId = 5, EjercicioId = 3, RespuestaTexto = "wrong" };

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(dto);

        // Assert
        Assert.False(resultado.EsCorrecta);
        Assert.Equal("Sigue practicando.", resultado.Mensaje);
    }

    [Fact]
    public async Task Evaluar_cuandoUsuarioAlcanza80PorcientoEn10Respuestas_subeDeNivel()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(100, "ok"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.A1));
        // 7 correctas previas + 1 incorrecta previa = 8, luego la nueva correcta hará 8/9? Necesitamos 8/10 con 80%
        // Sembramos 8 correctas y 1 incorrecta (9 totales), la décima será correcta => 9/10 = 90% -> sube
        var historicas = Enumerable.Range(0, 8).Select(i => new RespuestaUsuario
        {
            Id = i + 1,
            UsuarioId = 1,
            EjercicioId = 100,
            RespuestaTexto = "ok",
            EsCorrecta = true,
            FechaRealizacion = DateTime.UtcNow.AddMinutes(-10 + i)
        }).Concat(new[]
        {
            new RespuestaUsuario { Id = 9, UsuarioId = 1, EjercicioId = 100, RespuestaTexto = "bad", EsCorrecta = false, FechaRealizacion = DateTime.UtcNow.AddMinutes(-1) }
        });
        usuarioRepo.SembrarRespuestas(historicas);
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var dto = new RespuestaDto { UsuarioId = 1, EjercicioId = 100, RespuestaTexto = "ok" };

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(dto);

        // Assert
        Assert.True(resultado.EsCorrecta);
        Assert.Equal("A2", resultado.NuevoNivel);
        Assert.Contains("¡Felicidades! Has subido al nivel A2.", resultado.Mensaje);
        var usuario = await usuarioRepo.ObtenerPorIdAsync(1);
        Assert.Equal(NivelIngles.A2, usuario!.NivelActual);
    }

    [Fact]
    public async Task Evaluar_cuandoUsuarioYaEsC1_noSubeDeNivel()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(200, "ok"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.C1));
        var historicas = Enumerable.Range(0, 10).Select(i => new RespuestaUsuario
        {
            Id = i + 1,
            UsuarioId = 1,
            EjercicioId = 200,
            RespuestaTexto = "ok",
            EsCorrecta = true,
            FechaRealizacion = DateTime.UtcNow.AddMinutes(-i)
        });
        usuarioRepo.SembrarRespuestas(historicas);
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 1, EjercicioId = 200, RespuestaTexto = "ok" });

        // Assert
        Assert.True(resultado.EsCorrecta);
        Assert.Null(resultado.NuevoNivel);
        Assert.DoesNotContain("Felicidades", resultado.Mensaje);
        var usuario = await usuarioRepo.ObtenerPorIdAsync(1);
        Assert.Equal(NivelIngles.C1, usuario!.NivelActual);
    }

    [Fact]
    public async Task Evaluar_cuandoMenosDe10Respuestas_noSubeDeNivel_aunque100Porciento()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(300, "ok"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(1, NivelIngles.B1));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);

        // 9 respuestas correctas previas -> con la nueva son 10? Necesitamos menos de 10 totales tras guardar => 4 previas + nueva =5 (<10)
        var historicas = Enumerable.Range(0, 4).Select(i => new RespuestaUsuario
        {
            Id = i+1, UsuarioId = 1, EjercicioId = 300, RespuestaTexto = "ok", EsCorrecta = true, FechaRealizacion = DateTime.UtcNow.AddMinutes(-i)
        });
        usuarioRepo.SembrarRespuestas(historicas);

        // Act
        var resultado = await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 1, EjercicioId = 300, RespuestaTexto = "ok" });

        // Assert
        Assert.True(resultado.EsCorrecta);
        Assert.Null(resultado.NuevoNivel);
    }

    [Fact]
    public async Task Evaluar_persisteRespuestaUsuario_conDatosCorrectos()
    {
        // Arrange
        var leccionRepo = new FakeLeccionRepositorio();
        leccionRepo.AgregarEjercicio(CrearEjercicio(400, "expected"));
        var usuarioRepo = new FakeUsuarioRepositorio();
        usuarioRepo.AgregarUsuario(CrearUsuario(7, NivelIngles.A2));
        var sut = new LeccionServicio(leccionRepo, usuarioRepo);
        var before = DateTime.UtcNow;

        // Act
        await sut.EvaluarEjercicioAsync(new RespuestaDto { UsuarioId = 7, EjercicioId = 400, RespuestaTexto = "expected" });

        // Assert
        var guardada = usuarioRepo.RespuestasPersistidas.Single();
        Assert.Equal(7, guardada.UsuarioId);
        Assert.Equal(400, guardada.EjercicioId);
        Assert.Equal("expected", guardada.RespuestaTexto);
        Assert.True(guardada.EsCorrecta);
        Assert.True(guardada.FechaRealizacion >= before && guardada.FechaRealizacion <= DateTime.UtcNow.AddSeconds(1));
    }
}
