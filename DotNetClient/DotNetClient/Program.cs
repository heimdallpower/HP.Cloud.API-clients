using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace DotNetClient
{
    public class Program
    {
        // Step 1/4 - Insert your client id and certificate password here.  
        private const string ClientId = "ENTER_YOUR_VALUE_HERE"; // The identity of this application
        private const string CertificatePassword = "ENTER_YOUR_VALUE_HERE";

        // Step 2/4 - Add the certificate to the root of the solution
        // Step 3/4 - Make sure the file name matches this variable including the .pfx extension
        // Step 4/4 - Right click certificate --> properties --> set "Copy to output folder: always"
        private const string CertificatePath = "myCertificateName.pfx";

        // Other constants
        private const string ApiUrl = "https://hp-cloud-api-app-dev.azurewebsites.net/"; // Heimdall API URL
        private const string Authority =
            "https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073"; // Heimdall's Azure tenant
        private const string Scope = "971c3c3b-0b7c-4991-bc10-6ac424c58779/.default"; // Which scope does this application require? The id here is the Heimdall API's client id


        private static async Task Main()
        {
            Console.WriteLine("Hello Heimdall!");

            // Get an access token representing the identity of this application
            var accessToken = await GetAccessToken(ClientId);

            // Create a http client which forwards the access token
            var heimdallClient = GetHeimdallApiClient(accessToken);

            // Get data from the API 
            var neuronId = 717;
            var toDate = DateTime.Now;
            var fromDate = DateTime.Now.AddDays(-7);

            await GetMeasurementsForPowerLine(neuronId, fromDate, toDate, heimdallClient);
        }
        private static string GetCurrentDirectoryFromExecutingAssembly()
        {
            var codeBase = typeof(Program).GetTypeInfo().Assembly.CodeBase;

            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        private static async Task<string> GetAccessToken(string clientId)
        {
            Console.WriteLine("Retrieving access token...");
            var certPath = Path.Combine(GetCurrentDirectoryFromExecutingAssembly(), CertificatePath);
            var certfile = File.OpenRead(certPath);
            var certificateBytes = new byte[certfile.Length];
            certfile.Read(certificateBytes, 0, (int)certfile.Length);
            var cert = new X509Certificate2(
                certificateBytes,
                CertificatePassword,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.MachineKeySet |
                X509KeyStorageFlags.PersistKeySet);

            var certificate = new ClientAssertionCertificate(clientId, cert);
            AuthenticationContext context = new AuthenticationContext(Authority);
            AuthenticationResult authenticationResult = await context.AcquireTokenAsync(Scope, certificate);

            Console.WriteLine($"Token retrieved. The token will expire on {authenticationResult.ExpiresOn}");
            return authenticationResult.AccessToken;
        }

        private static HttpClient GetHeimdallApiClient(string accessToken)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(ApiUrl)
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return httpClient;
        }

        private static async Task GetMeasurementsForPowerLine(int neuronId, DateTime fromDate, DateTime toDate, HttpClient heimdallClient)
        {
            

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";

            var url = "api/measurements?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"neuronId={neuronId}";

            Console.WriteLine($"Sending request to {heimdallClient.BaseAddress}{url}");

            var response = await heimdallClient.GetAsync(url);
            Console.WriteLine($"Response: {response}");

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<MeasurementResponse>(jsonString);

                foreach (var measurement in measurementResponse.data)
                {
                    Console.WriteLine($"Measurement at {measurement.timeObserved}\n" +
                                      $"Current: {measurement.current}A\n" +
                                      $"Wire: {measurement.wireTemperature} degrees\n" +
                                      $"Housing: {measurement.housingTemperature} degrees\n");
                }
                Console.WriteLine($"Code: {measurementResponse.code} - {measurementResponse.message} - found {measurementResponse.data.Count} measurements");
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Request failed, exception: {exceptionString}");
            }
        }

 
    
    }

    public class TokenResponse
    {
        public string access_token;
        public string expires_in;
    }

    public class MeasurementResponse
    {
        public int code;
        public string message;
        public List<Measurement> data;
    }

    public class Measurement
    {
        public double current;
        public double wireTemperature;
        public double housingTemperature;
        public string timeObserved;
        public int neuronId;
    }
}
