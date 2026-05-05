using System.Net;
using System.Text.Json;
using GMForce.Bricks.Initialization.Http;
using GMForce.Bricks.Tests.Arrangement;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Http;

internal sealed class ClientExtensionsFixture
{
    private static JsonSerializerOptions CamelCase => new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    [Test]
    public async Task PostWithResponseAsyncSuccessDeserializes()
    {
        using var client = FakeMessageHandler.ClientWith(JsonSerializer.Serialize(new { value = "ok" }, CamelCase));

        var result = await client.PostWithResponseAsync<string, JsonElement>("http://test.example/api", "data");

        result.GetProperty("value").GetString().ShouldBe("ok");
    }

    [Test]
    public async Task PostWithResponseAsyncWithReferrerSetsHeader()
    {
        using var client = FakeMessageHandler.ClientWith(JsonSerializer.Serialize(new { value = "ok" }, CamelCase));
        var referrer = new Uri("https://referrer.example");

        _ = await client.PostWithResponseAsync<string, JsonElement>("http://test.example/api", "data", referrer);

        client.DefaultRequestHeaders.Referrer.ShouldBe(referrer);
    }

    [Test]
    public async Task PostWithResponseAsyncFailureStatusThrows()
    {
        using var client = FakeMessageHandler.ClientWith("{}", HttpStatusCode.BadRequest);

        Task act()
        {
            return client.PostWithResponseAsync<string, JsonElement>("http://test.example/api", "data");
        }

        await Should.ThrowAsync<HttpRequestException>(act);
    }

    [Test]
    public async Task PostWithResponseAsyncNullBodyThrows()
    {
        using var client = FakeMessageHandler.ClientWith("null");

        Task act()
        {
            return client.PostWithResponseAsync<string, string>("http://test.example/api", "data");
        }

        await Should.ThrowAsync<InvalidOperationException>(act);
    }

    [Test]
    public async Task PostBodylessResponseAsyncSuccessDeserializes()
    {
        using var client = FakeMessageHandler.ClientWith(JsonSerializer.Serialize(new { value = "ok" }, CamelCase));

        var result = await client.PostBodylessResponseAsync<JsonElement>("http://test.example/api");

        result.GetProperty("value").GetString().ShouldBe("ok");
    }
}
