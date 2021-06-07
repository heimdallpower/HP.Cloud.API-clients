using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace DotNetClient
{
    public class Program
    {
        private const string ClientId = "ENTER_CLIENT_ID";
        private const string ClientSecret = "ENTER_CLIENT_SECRET";
        private const string ApiUrl = "https://hp-cloud-api-app-dev.azurewebsites.net/";

        private static async Task Main()
        {
            Console.WriteLine("Hello Heimdall!");
            var neuronId = 717;
            var toDate = DateTime.Now;
            var fromDate = DateTime.Now.AddDays(-7);

            await GetLineMeasurements(neuronId, fromDate, toDate);
        }

        static async Task GetLineMeasurements(int neuronId, DateTime fromDate, DateTime toDate)
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(ApiUrl)
            };

            var accessToken = GetAccessToken();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";

            var url = "api/measurements?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"neuronId={neuronId}";

            Console.WriteLine($"Sending request to {httpClient.BaseAddress}{url}");

            var response = await httpClient.GetAsync(url);
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

        static string GetAccessToken()
        {
            Console.WriteLine("Retrieving access token...");
            var client = new RestClient("https://login.microsoftonline.com/132d3d43-145b-4d30-aaf3-0a47aa7be073/oauth2/v2.0/token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("client_id", ClientId);
            request.AddParameter("client_secret", ClientSecret);
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", "971c3c3b-0b7c-4991-bc10-6ac424c58779/.default");

            IRestResponse response = client.Execute(request);
            var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response.Content);

            Console.WriteLine($"Token retrieved. The token will expire in {tokenResponse.expires_in} seconds. \n{tokenResponse.access_token}");
            return tokenResponse.access_token;
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
