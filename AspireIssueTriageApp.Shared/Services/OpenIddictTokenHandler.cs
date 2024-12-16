using System.Net.Http.Headers;
using OpenIddict.Client;

namespace AspireIssueTriageApp.Services;

public class OpenIddictTokenHandler(OpenIddictClientService _clientService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await _clientService.AuthenticateWithClientCredentialsAsync(new());

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResult.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}