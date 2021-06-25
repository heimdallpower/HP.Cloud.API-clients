using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DotNetClient.Entities;
using DotNetClient.Enums;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;

namespace DotNetClient
{
    public class Program
    {
        private const string ClientId = "INSERT_VARIABLE_HERE";
        private const string PfxCertificatePath = "INSERT_VARIABLE_HERE";
        private const string CertificatePassword = "INSERT_VARIABLE_HERE";

        // Other constants
        private const string ApiUrl = "https://api.heimdallcloud.com"; // Heimdall API URL
        private const string Authority =
            "https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073"; // Heimdall's Azure tenant
        private const string Scope = "8ecd41dd-9d79-4440-b029-8ea602733d60/.default"; // Which scope does this application require? The id here is the Heimdall API's client id


        private static async Task Main()
        {
            Console.WriteLine("Hello Heimdall!");

            // Create a http client which forwards the access token
            var heimdallClient = await GetHeimdallApiClient();

            var lineNames = await GetLineNames(heimdallClient);

            if (lineNames.Count <= 0)
            {
                Console.WriteLine("Didn't find any lines");
            }
            else
            {
                var chosenLine = lineNames[0];
                var toDate = DateTime.Now;
                var fromDate = DateTime.Now.AddDays(-7);

                await GetAggregatedMeasurementsForLine(chosenLine, fromDate, toDate, heimdallClient);
            }
           
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

            var certificate = new ClientAssertionCertificate(clientId, cert);
            AuthenticationContext context = new AuthenticationContext(Authority);
            AuthenticationResult authenticationResult = await context.AcquireTokenAsync(Scope, certificate);

            Console.WriteLine($"Token retrieved. The token will expire on {authenticationResult.ExpiresOn}\n{authenticationResult.AccessToken}");
            return authenticationResult.AccessToken;
        }

        private static async Task<HttpClient> GetHeimdallApiClient()
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(ApiUrl)
            };

            // Get an access token representing the identity of this application
            var accessToken = await GetAccessToken(ClientId);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return httpClient;
        }

        private static async Task<List<string>> GetLineNames(HttpClient heimdallClient)
        {
            var response = await heimdallClient.GetAsync("api/beta/lines");
            Console.WriteLine($"Response: {response}");

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var linesResponse = JsonConvert.DeserializeObject<LineResponse>(jsonString);

                foreach(var lineDto in linesResponse.data)
                {
                    Console.WriteLine($"You can request data for line: {lineDto.name}\n" +
                                      $"Owner: {lineDto.owner}\n" +
                                      $"Line spans in line:\n{string.Join("\n", lineDto.lineSpans.Select(lineSpan => lineSpan.name))}\n");
                }
                var lineNames = linesResponse.data.Select(line => line.name).ToList();
                return lineNames;
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Lines request failed, exception: {exceptionString}");
            }
            return new List<string>();

        }
        private static async Task GetAggregatedMeasurementsForLine(string lineName, DateTime fromDate, DateTime toDate, HttpClient heimdallClient)
        {
            Console.WriteLine($"Requesting data for line: {lineName}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            // By including the optional lineSpanName
            var url = "api/beta/aggregated-measurements?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"measurementType={MeasurementType.Current}&" +
                      $"aggregationType={AggregationType.Max}&" +
                      $"lineName={lineName}";

            Console.WriteLine($"Sending request to {heimdallClient.BaseAddress}{url}");

            var response = await heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {jsonString}");

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<AggregatedMeasurementsResponse>(jsonString);

                foreach (var measurement in measurementResponse.data)
                {
                    Console.WriteLine($"Current at {DateTime.Parse(measurement.intervalStartTime)}: {measurement.value}A\n");
                }
                Console.WriteLine($"Response: {measurementResponse.message} - found {measurementResponse.data.Count} measurements");
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Request failed, exception: {exceptionString}");
            }
        }
    }

  




}