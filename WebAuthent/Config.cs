using System.Collections.Generic;
using IdentityServer4.Models;

namespace WebAuthent
{
    public class Config
    {
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>();
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("testApi", "My test api")
            };
        }
    }
}
