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
        protected HttpClient HttpClient { get; } = new HttpClient() { BaseAddress = new Uri(ApiUrl) };
        private readonly string ClientID;
        private readonly string PfxCertificatePath;
        private readonly string CertificatePassword;

        private const string ApiUrl = "https://api.heimdallcloud.com"; // Heimdall API URL
        private const string Authority =
            "https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073"; // Heimdall's Azure tenant
        private const string Scope = "aac6dec0-4c1b-4565-a825-5bb9401a1547/.default"; // Which scope does this application require? The id here is the Heimdall API's client id

        public HeimdallHttpClient(string clientID, string pfxCertificatePath, string certificatePassword)
        {
            ClientID = clientID;
            PfxCertificatePath = pfxCertificatePath;
            CertificatePassword = certificatePassword;
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
            var certPath = Path.Combine(GetCurrentDirectoryFromExecutingAssembly(), PfxCertificatePath);
            var certfile = File.OpenRead(certPath);
            var certificateBytes = new byte[certfile.Length];
            certfile.Read(certificateBytes, 0, (int)certfile.Length);
            var cert = new X509Certificate2(
                certificateBytes,
                CertificatePassword,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet);

            var certificate = new ClientAssertionCertificate(ClientID, cert);
            AuthenticationContext context = new AuthenticationContext(Authority);
            AuthenticationResult authenticationResult = await context.AcquireTokenAsync(Scope, certificate);

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
