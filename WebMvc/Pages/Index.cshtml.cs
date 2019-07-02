using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace WebMvc.Pages
{
    public class IndexModel : PageModel
    {
        public async Task OnGet()
        {
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true
            };
            var token = await ContactAuthentServ(handler);
            if (token is null) return;
            
            var client = new HttpClient(handler)
            {
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
            };
            client.SetBearerToken(token);
            var response = await client.GetAsync("https://webapi:443/api/values");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }

        private static async Task<string> ContactAuthentServ(HttpClientHandler handler)
        {
            var client = new HttpClient(handler);
            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest{
                Address = "https://webauthent:443",
                Policy =
                {
                    ValidateIssuerName = false,
                    ValidateEndpoints = false
                }
            });
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return null;
            }

            // request token
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "webmvcclient",
                ClientSecret = "secret"
            });

            if (response.IsError)
            {
                Console.WriteLine(response.Error);
                return null;
            }

            Console.WriteLine(response.Json);

            return response.AccessToken;
        }
    }
}
