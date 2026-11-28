using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace EnglishLearning.Tests.Integration;

public sealed class PostgreSqlFactAttribute : FactAttribute
{
    public PostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EnglishLearningTestDatabase")))
            Skip = "EnglishLearningTestDatabase debe identificar un PostgreSQL de pruebas aislado.";
    }
}

public sealed class UserJourneyTests
{
    private const string Password = "English-Tests1";
    private const string NewPassword = "English-Tests2";

    [PostgreSqlFact]
    public async Task AccountLifecycleVerifiesEmailRotatesTokensAndRevokesSessionsOnReset()
    {
        await using var app = new TestApplication();
        using var browser = app.CreateClient();
        var email = NewEmail();
        var tokens = await RegisterAsync(browser, app.Mail, email);

        using var duplicate = await browser.PostAsJsonAsync("/api/v1/auth/register", new { nombre = "Student", email, password = Password });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        var renewal = new { refreshToken = tokens.GetProperty("refreshToken").GetString() };
        var concurrent = await Task.WhenAll(
            browser.PostAsJsonAsync("/api/v1/auth/refresh", renewal),
            browser.PostAsJsonAsync("/api/v1/auth/refresh", renewal));
        var successfulRenewal = Assert.Single(concurrent, response => response.StatusCode == HttpStatusCode.OK);
        Assert.Contains(concurrent, response => response.StatusCode == HttpStatusCode.Unauthorized);
        var rotated = await ReadDataAsync(successfulRenewal);
        foreach (var response in concurrent)
            response.Dispose();

        SetAuthorization(browser, rotated);
        using var me = await browser.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);

        var recovery = await PostAsync(browser, "/api/v1/auth/request-password-reset", new { email });
        Assert.Equal(JsonValueKind.Null, recovery.GetProperty("accessToken").ValueKind);
        await PostAsync(browser, "/api/v1/auth/reset-password", new
        {
            challengeId = recovery.GetProperty("challengeId").GetString(),
            code = app.Mail.CodeFor(email),
            password = NewPassword
        });

        using var staleAccess = await browser.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, staleAccess.StatusCode);
        using var staleRefresh = await browser.PostAsJsonAsync("/api/v1/auth/refresh",
            new { refreshToken = rotated.GetProperty("refreshToken").GetString() });
        Assert.Equal(HttpStatusCode.Unauthorized, staleRefresh.StatusCode);

        using var invalidPassword = await browser.PostAsJsonAsync("/api/v1/auth/login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidPassword.StatusCode);

        var login = await PostAsync(browser, "/api/v1/auth/login", new { email, password = NewPassword });
        Assert.True(login.GetProperty("requiresOtp").GetBoolean());
        var newSession = await VerifyAsync(browser, app.Mail, email, login);
        SetAuthorization(browser, newSession);
        using var renewedMe = await browser.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.OK, renewedMe.StatusCode);
    }

    [PostgreSqlFact]
    public async Task DiagnosisCatalogPracticeAndProgressShareAuthenticatedState()
    {
        await using var app = new TestApplication();
        using var browser = app.CreateClient();
        var tokens = await RegisterAsync(browser, app.Mail, NewEmail());
        SetAuthorization(browser, tokens);

        using var noLevel = await browser.GetAsync("/api/v1/lessons");
        Assert.Equal(HttpStatusCode.Forbidden, noLevel.StatusCode);
        var session = await GetAsync(browser, "/api/v1/assessments/questions");
        var attempt = session.GetProperty("attempt").GetString();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnglishLearningDbContext>();
        var questions = await db.PreguntasDiagnosticas.AsNoTracking().ToListAsync();
        var answers = questions.Select(q => new { preguntaId = q.Id, respuestaTexto = "Incorrect answer" }).ToArray();
        using var partial = await browser.PostAsJsonAsync("/api/v1/assessments/submit", new { attempt, respuestas = answers.Take(8) });
        Assert.Equal(HttpStatusCode.BadRequest, partial.StatusCode);

        var placement = await PostAsync(browser, "/api/v1/assessments/submit", new { attempt, respuestas = answers });
        Assert.Equal("A1", placement.GetProperty("nivelAsignado").GetString());
        using var otherLevel = await browser.GetAsync("/api/v1/lessons/by-level/A2");
        Assert.Equal(HttpStatusCode.Forbidden, otherLevel.StatusCode);
        var catalog = await GetAsync(browser, "/api/v1/lessons");
        Assert.Equal(10, catalog.GetArrayLength());
        Assert.Equal(42, catalog.EnumerateArray().Sum(lesson => lesson.GetProperty("ejercicios").GetArrayLength()));

        var lessons = await db.Lecciones.AsNoTracking().Include(l => l.Ejercicios)
            .Where(l => l.NivelRequerido == EnglishLearning.Domain.Enums.NivelIngles.A1).ToListAsync();
        var regexAnswers = new Dictionary<string, string>
        {
            ["Preséntate usando el nombre Alex en inglés."] = "Hello, my name is Alex.",
            ["Traduce: Ella trabaja los lunes."] = "She works on Mondays."
        };
        JsonElement result = default;
        var submittedAnswers = lessons.SelectMany(l => l.Ejercicios).OrderBy(e => e.Id).ToArray();
        foreach (var exercise in submittedAnswers)
        {
            var text = exercise.EstrategiaEvaluacion == "Regex" ? regexAnswers[exercise.Contenido] : exercise.RespuestaCorrecta;
            result = await PostAsync(browser, "/api/v1/lessons/evaluate", new { ejercicioId = exercise.Id, respuestaTexto = text });
            Assert.True(result.GetProperty("esCorrecta").GetBoolean());
            if (exercise.Id != submittedAnswers[^1].Id)
                Assert.Equal(JsonValueKind.Null, result.GetProperty("nuevoNivel").ValueKind);
        }

        Assert.Equal("A2", result.GetProperty("nuevoNivel").GetString());
        var progress = await GetAsync(browser, "/api/v1/progress");
        var profile = await GetAsync(browser, "/api/v1/auth/me");
        Assert.Equal(42, progress.GetProperty("practice").GetProperty("total").GetInt32());
        Assert.Equal(progress.GetProperty("practice").GetRawText(), profile.GetProperty("practice").GetRawText());
        var a1 = Assert.Single(progress.GetProperty("progresos").EnumerateArray(), level => level.GetProperty("nivel").GetString() == "A1");
        Assert.Equal(100, a1.GetProperty("porcentajeCompletado").GetDouble());
        Assert.Equal(10, a1.GetProperty("leccionesCompletadas").GetInt32());
    }

    [PostgreSqlFact]
    public async Task CatalogHasTenLessonsPerLevelAndSeedingPreservesIds()
    {
        await using var app = new TestApplication();
        using var browser = app.CreateClient();
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EnglishLearningDbContext>();
        var before = await db.Lecciones.AsNoTracking().Include(l => l.Ejercicios)
            .OrderBy(l => l.Id).ToListAsync();

        var levels = before.GroupBy(l => l.NivelRequerido).ToArray();
        Assert.Equal(5, levels.Length);
        foreach (var level in levels)
        {
            Assert.Equal(10, level.Count());
            Assert.Equal(42, level.Sum(l => l.Ejercicios.Count));
            Assert.Equal(10, level.Select(l => l.Titulo).Distinct().Count());
            foreach (var exercise in level.SelectMany(l => l.Ejercicios)
                         .Where(e => e.Contenido.Contains('[')))
            {
                var opening = exercise.Contenido.IndexOf('[');
                var closing = exercise.Contenido.IndexOf(']', opening);
                Assert.Contains(exercise.RespuestaCorrecta, exercise.Contenido[(opening + 1)..closing].Split('|'));
            }
        }

        await scope.ServiceProvider.GetRequiredService<EnglishLearning.Infrastructure.Services.ISeedDataService>().SeedAsync();
        var after = await db.Lecciones.AsNoTracking().Include(l => l.Ejercicios)
            .OrderBy(l => l.Id).ToListAsync();

        Assert.Equal(before.Select(l => l.Id), after.Select(l => l.Id));
        Assert.Equal(before.SelectMany(l => l.Ejercicios).Select(e => e.Id).Order(),
            after.SelectMany(l => l.Ejercicios).Select(e => e.Id).Order());
    }

    [PostgreSqlFact]
    public async Task TutorUsesServerLevelRespectsConsentAndReportsUnavailable()
    {
        await using var app = new TestApplication();
        using var browser = app.CreateClient();
        SetAuthorization(browser, await RegisterAsync(browser, app.Mail, NewEmail()));

        var result = await PostAsync(browser, "/api/v1/chat/ask", new
        {
            message = "Can you explain that again?",
            history = new[] { new { isStudent = true, text = "Explain the past simple." }, new { isStudent = false, text = "It describes completed actions." } }
        });
        Assert.Equal("sin diagnóstico · 2", result.GetProperty("response").GetString());

        using var consentRequired = await browser.PostAsJsonAsync("/api/v1/chat/production-feedback",
            new { consent = false, writing = "I made a decision after reviewing the options." });
        Assert.Equal(HttpStatusCode.BadRequest, consentRequired.StatusCode);
        var feedback = await PostAsync(browser, "/api/v1/chat/production-feedback",
            new { consent = true, writing = "I made a decision after reviewing the options." });
        Assert.Equal(3, feedback.GetProperty("writing")[0].GetProperty("score").GetInt32());

        using var failure = await browser.PostAsJsonAsync("/api/v1/chat/ask", new { message = "simulate-unavailable" });
        Assert.Equal(HttpStatusCode.ServiceUnavailable, failure.StatusCode);
        var body = await failure.Content.ReadAsStringAsync();
        Assert.Contains("correlationId", body);
        Assert.DoesNotContain("provider internal marker", body);
    }

    private static string NewEmail() => $"student-{Guid.NewGuid():N}@example.test";

    private static async Task<JsonElement> RegisterAsync(HttpClient browser, CapturedMail mail, string email)
    {
        var pending = await PostAsync(browser, "/api/v1/auth/register", new { nombre = "Student", email, password = Password });
        Assert.True(pending.GetProperty("requiresOtp").GetBoolean());
        Assert.Equal(JsonValueKind.Null, pending.GetProperty("accessToken").ValueKind);
        return await VerifyAsync(browser, mail, email, pending);
    }

    private static Task<JsonElement> VerifyAsync(HttpClient browser, CapturedMail mail, string email, JsonElement pending) =>
        PostAsync(browser, "/api/v1/auth/verify-otp", new { challengeId = pending.GetProperty("challengeId").GetString(), code = mail.CodeFor(email) });

    private static void SetAuthorization(HttpClient browser, JsonElement tokens) =>
        browser.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.GetProperty("accessToken").GetString());

    private static async Task<JsonElement> GetAsync(HttpClient browser, string route)
    {
        using var response = await browser.GetAsync(route);
        return await ReadDataAsync(response);
    }

    private static async Task<JsonElement> PostAsync(HttpClient browser, string route, object input)
    {
        using var response = await browser.PostAsJsonAsync(route, input);
        return await ReadDataAsync(response);
    }

    private static async Task<JsonElement> ReadDataAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode} · {response.RequestMessage?.RequestUri?.AbsolutePath} · {body}");
        using var payload = JsonDocument.Parse(body);
        return payload.RootElement.GetProperty("data").Clone();
    }

    private sealed class TestApplication : WebApplicationFactory<Program>
    {
        private readonly string? _priorConnection;
        private readonly string? _priorJwt;

        public CapturedMail Mail { get; } = new();

        public TestApplication()
        {
            var connection = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("EnglishLearningTestDatabase"));
            if (connection.Host is not ("localhost" or "127.0.0.1" or "::1"))
                throw new InvalidOperationException("Integration tests require a local PostgreSQL instance.");

            _priorConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            _priorJwt = Environment.GetEnvironmentVariable("Jwt__SecretKey");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connection.ConnectionString);
            Environment.SetEnvironmentVariable("Jwt__SecretKey", "integration-test-only-key-173a364e943241d7a4d9bbdad7cc1a4d");
        }

        public override async ValueTask DisposeAsync()
        {
            try { await base.DisposeAsync(); }
            finally
            {
                Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _priorConnection);
                Environment.SetEnvironmentVariable("Jwt__SecretKey", _priorJwt);
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedData:AutoSeedOnStartup"] = "true",
                ["IpRateLimiting:EnableEndpointRateLimiting"] = "false",
                ["IpRateLimiting:GeneralRules:0:Limit"] = "500"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IEmailService>();
                services.AddSingleton<IEmailService>(Mail);
                services.RemoveAll<IChatService>();
                services.AddSingleton<IChatService, TestTutor>();
            });
        }
    }

    private sealed class CapturedMail : IEmailService
    {
        private readonly ConcurrentDictionary<string, string> _codes = new();
        public string CodeFor(string address) => _codes[address];

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            _codes[to] = Assert.Single(Regex.Matches(body, @">\s*([0-9]{6})\s*<").Select(m => m.Groups[1].Value));
            return Task.CompletedTask;
        }
    }

    private sealed class TestTutor : IChatService
    {
        public Task<string> SendMessageAsync(string message, string level, IReadOnlyList<TutorTurn> history, CancellationToken ct) =>
            message == "simulate-unavailable"
                ? Task.FromException<string>(new TutorUnavailableException("provider internal marker"))
                : Task.FromResult($"{level} · {history.Count}");

        public Task<TutorRubric> EvaluateProductionAsync(string? writing, byte[]? wav, string level, CancellationToken ct) =>
            Task.FromResult(new TutorRubric("Buen comienzo.", [new RubricCriterion("Organización", 3, "Usa conectores.")], []));
    }
}
