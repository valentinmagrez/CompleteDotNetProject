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
            var token = await ContactAuthentServ();
            if (token is null) return;
            
            var client = new HttpClient
            {
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
            };
            client.SetBearerToken(token);
            var response = await client.GetAsync("http://webapi:80/api/values");
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

        private static async Task<string> ContactAuthentServ()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest{
                Address = "http://webauthent:80",
                Policy =
                {
                    ValidateIssuerName = false,
                    ValidateEndpoints = false,
                    RequireHttps = false
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
