using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // discover endpoints from metadata
            // 从http://localhost:5000/.well-known/openid-configuration获取元数据和endpoint
            // 只需要填写IdentityServer的基本地址[http(s)://domain]即可
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("http://localhost:5000");
            if (disco.IsError)
            {
                //Keyset is missing, 
                //报这个错，IdentityServer项目下缺少tempkey.rsa文件，AddDeveloperSigningCredential
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            // 从元数据获取到的token_endpoint请求令牌
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            });
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);

            // call api
            // 使用扩展方法SetBearerToken将AccessToken添加到HTTP Authorization标头，然后请求api
            //var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);
            var response = await client.GetAsync("http://localhost:5001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
            Console.ReadLine();
        }
    }
}
