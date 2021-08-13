using DotNetClient.Clients;
using DotNetClient.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetClient
{
    public class Program
    {
        private const string ClientId = "INSERT_VARIABLE_HERE";
        private const string PfxCertificatePath = "INSERT_VARIABLE_HERE";
        private const string CertificatePassword = "INSERT_VARIABLE_HERE";

        private static async Task Main()
        {
            Console.WriteLine("Hello Heimdall!");

            var heimdallClient = await new HeimdallHttpClient(ClientId, PfxCertificatePath, CertificatePassword).GetHttpClient();

            // Create a http client which forwards the access token to the Heimdall API. For production usage of HttpClient, see this https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            var service = new HeimdallService(heimdallClient);

            while (true)
            {
                await service.Run();
                Thread.Sleep(10 * 1000);
            }
        }
    }
}