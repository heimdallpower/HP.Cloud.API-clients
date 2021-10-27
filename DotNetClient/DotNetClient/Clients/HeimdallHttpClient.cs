using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DotNetClient.Clients
{
    public class HeimdallHttpClient
    {
        protected HttpClient HttpClient { get; }
        private readonly string _clientId;
        private readonly string _pfxCertificatePath;
        private readonly string _certificatePassword;
        private readonly string _scope; // Which scope does this application require? The id here is the Heimdall API's client id

        private const string ProdApiUrl = "https://api.heimdallcloud.com"; // Heimdall Prod API URL
        private const string DevApiUrl = "https://api.heimdallcloud-dev.com"; // Heimdall Dev API URL
        private const string Authority = "https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073"; // Heimdall's Azure tenant
        private const string ProdScope = "aac6dec0-4c1b-4565-a825-5bb9401a1547/.default"; // Which scope does this application require? The id here is the Heimdall API's client id
        private const string DevScope = "6b9ba5c0-4a21-4263-bbf5-8c4e30c0ee1b/.default"; // Which scope does this application require? The id here is the Heimdall API's client 

        public HeimdallHttpClient(string clientId, string pfxCertificatePath, string certificatePassword, bool useDeveloperApi)
        {
            _clientId = clientId;
            _pfxCertificatePath = pfxCertificatePath;
            _certificatePassword = certificatePassword;
            _scope = useDeveloperApi ? DevScope : ProdScope;

            var apiUrl = useDeveloperApi ? DevApiUrl : ProdApiUrl;
            HttpClient = new() { BaseAddress = new Uri(apiUrl) };
        }

        public async Task<HttpClient> GetHttpClient()
        {
            var accessToken = await GetAccessToken();
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return HttpClient;
        }

        private async Task<string> GetAccessToken()
        {
            Console.WriteLine("Retrieving access token...");
            var certPath = Path.Combine(GetCurrentDirectoryFromExecutingAssembly(), _pfxCertificatePath);
            var certfile = File.OpenRead(certPath);
            var certificateBytes = new byte[certfile.Length];
            certfile.Read(certificateBytes, 0, (int)certfile.Length);
            var cert = new X509Certificate2(
                certificateBytes,
                _certificatePassword,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet);

            var certificate = new ClientAssertionCertificate(_clientId, cert);
            AuthenticationContext context = new AuthenticationContext(Authority);
            AuthenticationResult authenticationResult = await context.AcquireTokenAsync(_scope, certificate);

            Console.WriteLine($"Token retrieved. The token will expire on {authenticationResult.ExpiresOn}\n{authenticationResult.AccessToken}");
            return authenticationResult.AccessToken;
        }

        private static string GetCurrentDirectoryFromExecutingAssembly()
        {
            var codeBase = typeof(Program).GetTypeInfo().Assembly.Location;

            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
