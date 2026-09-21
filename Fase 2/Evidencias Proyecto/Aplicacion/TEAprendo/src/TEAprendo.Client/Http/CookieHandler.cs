using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace TEAprendo.Client.Http;

// Envía la cookie de sesión en las llamadas a la API.
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(
            BrowserRequestCredentials.Include);

        request.Headers.TryAddWithoutValidation(
            "X-Requested-With",
            "XMLHttpRequest");

        return base.SendAsync(request, cancellationToken);
    }
}