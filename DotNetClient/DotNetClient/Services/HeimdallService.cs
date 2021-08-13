using DotNetClient.Entities;
using DotNetClient.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNetClient.Services
{
    public class HeimdallService
    {
        private readonly HttpClient heimdallClient;
        private DateTime toDate;
        private DateTime fromDate;

        public HeimdallService(HttpClient httpClient)
        {
            heimdallClient = httpClient;
            toDate = DateTime.Now;
            fromDate = DateTime.Now.AddDays(-7); 
        }

        public async Task Run()
        {
            toDate = DateTime.Now;
            fromDate = DateTime.Now.AddDays(-7); 

            var lineNames = await GetLineNames();
            if (!lineNames.Any())
                Console.WriteLine("Didn't find any lines");
            foreach(var lineName in lineNames)
            {
                await GetAggregatedMeasurementsForLine(lineName);
                await GetDynamicLineRatingsForLine(lineName);
            }
        }

        public async Task<List<string>> GetLineNames()
        {
            var response = await heimdallClient.GetAsync("api/beta/lines");
            Console.WriteLine($"Response: {response}");

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var linesResponse = JsonConvert.DeserializeObject<LineResponse>(jsonString);

                foreach (var lineDto in linesResponse.data)
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
                return new List<string>();
            }
        }

        public async Task GetAggregatedMeasurementsForLine(string lineName)
        {
            Console.WriteLine($"Requesting measurements for line: {lineName}");

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
                var measurementResponse = JsonConvert.DeserializeObject<ApiResponse<AggregatedMeasurementDto>>(jsonString);

                foreach (var measurement in measurementResponse.data)
                {
                    Console.WriteLine($"Current at {DateTime.Parse(measurement.intervalStartTime)}: {measurement.value} A\n");
                }
                Console.WriteLine($"Measurement response: {measurementResponse.message} - found {measurementResponse.data.Count} measurements");
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Measurement request failed, exception: {exceptionString}");
            }
        }

        public async Task GetDynamicLineRatingsForLine(string lineName, DLRType dlrType = DLRType.HP)
        {
            Console.WriteLine($"\nRequesting DLR for line: {lineName}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            // By including the optional lineSpanName
            var url = "api/beta/aggregated-dlr?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"dlrType={dlrType}&" +
                      $"lineName={lineName}";

            Console.WriteLine($"Sending request to {heimdallClient.BaseAddress}{url}");

            var response = await heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {jsonString}");

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<ApiResponse<DynamicLineRatingDto>>(jsonString);

                foreach (var dynamicLineRating in measurementResponse.data)
                {
                    Console.WriteLine($"Dynamic line rating at {DateTime.Parse(dynamicLineRating.intervalStartTime)}: {dynamicLineRating.ampacity} A\n");
                }
                Console.WriteLine($"DLR response: {measurementResponse.message} - found {measurementResponse.data.Count} DLRs");
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"DLR request failed, exception: {exceptionString}");
            }
        }
    }
}
