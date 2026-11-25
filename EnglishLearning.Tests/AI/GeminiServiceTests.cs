using System.Net;
using System.Text;
using System.Text.Json;
using EnglishLearning.Application.Abstractions;
using EnglishLearning.Application.Options;
using EnglishLearning.Infrastructure.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnglishLearning.Tests.AI;

public sealed class GeminiServiceTests
{
    [Fact]
    public async Task GeminiUsesFallbackAndSendsLevelAndContext()
    {
        var handler = new CaptureHandler(
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            ProviderResponse("I can explain it again."));
        using var client = new HttpClient(handler);
        var service = Create(client);
        var answer = await service.SendMessageAsync("Say that again?", "A2",
            [new(true, "What is an adverb?"), new(false, "It modifies a verb.")], CancellationToken.None);

        Assert.Equal("I can explain it again.", answer);
        Assert.Equal(["primary:generateContent", "fallback:generateContent"], handler.Models);
        using var payload = JsonDocument.Parse(handler.Payloads[0]);
        Assert.Contains("A2", payload.RootElement.GetProperty("system_instruction").GetRawText());
        Assert.Equal(3, payload.RootElement.GetProperty("contents").GetArrayLength());
        Assert.All(handler.Payloads, body => Assert.DoesNotContain("placeholder-test-key", body));
    }

    [Fact]
    public async Task ProductionSendsAudioAndDeserializesRubric()
    {
        const string rubric = """{"overview":"Practica conectores.","writing":[{"name":"Organización","score":3,"feedback":"Añade un ejemplo."}],"speaking":[{"name":"Claridad","score":4,"feedback":"Ritmo claro."}]}""";
        var handler = new CaptureHandler(ProviderResponse(rubric));
        using var client = new HttpClient(handler);
        var service = Create(client);
        var response = await service.EvaluateProductionAsync("I chose to practise after class.", [0, 1, 2],
            "B1", CancellationToken.None);

        Assert.Equal(3, Assert.Single(response.Writing).Score);
        Assert.Equal(4, Assert.Single(response.Speaking).Score);
        Assert.Contains("audio/wav", Assert.Single(handler.Payloads));
    }

    [Fact]
    public async Task GeminiClientReturnsControlledErrorOnMissingPayload()
    {
        var handler = new CaptureHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        using var client = new HttpClient(handler);
        var service = Create(client);
        await Assert.ThrowsAsync<TutorUnavailableException>(() =>
            service.SendMessageAsync("Hello", "A1", [], CancellationToken.None));
    }

    private static GeminiChatService Create(HttpClient client) =>
        new(client, Options.Create(new GeminiSettings
        {
            ApiKey = "placeholder-test-key",
            Model = "primary",
            FallbackModel = "fallback"
        }), NullLogger<GeminiChatService>.Instance);

    private static HttpResponseMessage ProviderResponse(string text) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(JsonSerializer.Serialize(new
        {
            candidates = new[] { new { content = new { parts = new[] { new { text } } } } }
        }), Encoding.UTF8, "application/json")
    };

    private sealed class CaptureHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        public List<string> Models { get; } = [];
        public List<string> Payloads { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Models.Add(request.RequestUri!.Segments[^1]);
            Assert.Empty(request.RequestUri.Query);
            Assert.Equal("placeholder-test-key", Assert.Single(request.Headers.GetValues("x-goog-api-key")));
            Payloads.Add(await request.Content!.ReadAsStringAsync(ct));
            return responses[Models.Count - 1];
        }
    }
}
