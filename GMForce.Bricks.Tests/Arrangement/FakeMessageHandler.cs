using System.Net;
using System.Text;

namespace GMForce.Bricks.Tests.Arrangement;

internal sealed class FakeMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    internal static HttpClient ClientWith(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var message = new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return new HttpClient(new FakeMessageHandler(message));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(response);
}
