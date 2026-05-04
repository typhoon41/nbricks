namespace GMForce.Bricks.Tests.Arrangement;

internal sealed class FakeMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response);
}
