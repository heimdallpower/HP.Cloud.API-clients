using DotNetClient.Clients;
using DotNetClient.Services;
using System;
using System.Threading.Tasks;

namespace DotNetClient
{
    public class Program
    {
        private const string ClientId = "INSERT_VARIABLE_HERE";
        private const string PfxCertificatePath = "INSERT_VARIABLE_HERE";
        private const string CertificatePassword = "INSERT_VARIABLE_HERE";

        private static async Task Main(string[] args)
        {
            if (!(args.Length == 0 || args.Length == 3))
            {
                Console.WriteLine("Program only supports 0 or 3 arguments, argument order is: ClientId PfxCertificatePath CertificatePassword");
                return;
            }

            HeimdallHttpClient heimdallHttpClient;
            if (args.Length == 3)
                heimdallHttpClient = new HeimdallHttpClient(args[0], args[1], args[2]);
            else
                heimdallHttpClient = new HeimdallHttpClient(ClientId, PfxCertificatePath, CertificatePassword);

            Console.WriteLine("Hello Heimdall!");


            var heimdallClient = await heimdallHttpClient.GetHttpClient();

            // Create a http client which forwards the access token to the Heimdall API. For production usage of HttpClient, see this https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
            var service = new HeimdallService(heimdallClient);

            await service.Run();
        }
    }
}