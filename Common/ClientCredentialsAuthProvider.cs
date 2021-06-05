using System;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace TeamsPresenceLight_PhilipsHue.Common
{
    public class ClientCredentialsAuthProvider : IAuthenticationProvider
    {
        private IConfidentialClientApplication _msalClient;
        private string[] _scopes;
        private string clientSecret = Configuration.AzureAdAppSecret;
        static string tenantId = Configuration.TenantId;
        static Uri authority = new Uri($"https://login.microsoftonline.com/{tenantId}/");

        public ClientCredentialsAuthProvider(string appId, string[] scopes)
        {
            _scopes = scopes;

            _msalClient = ConfidentialClientApplicationBuilder
                .Create(appId)
                .WithAuthority(authority)
                .WithClientSecret(clientSecret)
                .Build();
        }

        public async Task<string> GetAccessToken()
        {
            // If there is no saved user account, the user must sign-in
            try
            {
                var result = await _msalClient.AcquireTokenForClient(_scopes).ExecuteAsync();
                return result.AccessToken;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error getting access token: {exception.Message}");
                return null;
            }

        }

        // This is the required function to implement IAuthenticationProvider
        // The Graph SDK will call this function each time it makes a Graph
        // call.
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}
