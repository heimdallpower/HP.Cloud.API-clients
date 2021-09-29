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

            var lines = await GetLines();
            if (!lines.Any())
                Console.WriteLine("Didn't find any lines");
            foreach(var line in lines)
            {
                await GetAggregatedMeasurementsForLine(line);
                await GetDynamicLineRatingsForLine(line);
            }
        }

        public async Task<List<LineDto>> GetLines()
        {
            var response = await heimdallClient.GetAsync("api/v1/lines");
            Console.WriteLine($"Response: {response}");

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var linesResponse = JsonConvert.DeserializeObject<LineResponse>(jsonString);
                Console.WriteLine($"Use ids from this response to request Data\n{JsonConvert.SerializeObject(linesResponse.Data, Formatting.Indented)}\n");
                return linesResponse.Data;
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Lines request failed, exception: {exceptionString}");
                return new List<LineDto>();
            }
        }

        public async Task GetAggregatedMeasurementsForLine(LineDto line)
        {
            Console.WriteLine($"Requesting measurements for line: {line.Name}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            var url = "api/v1/aggregated-measurements?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"measurementType={MeasurementType.Current}&" +
                      $"aggregationType={AggregationType.Max}&" +
                      $"lineId={line.Id}";

            Console.WriteLine($"Sending request to {heimdallClient.BaseAddress}{url}");

            var response = await heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {jsonString}");

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<ApiResponse<AggregatedFloatValueDto>>(jsonString);
                var measurements = measurementResponse.Data;
                foreach (var measurement in measurements)
                {
                    Console.WriteLine($"Current at {measurement.IntervalStartTime}: {measurement.Value} A\n");
                }
                Console.WriteLine($"Measurement response: {measurementResponse.Message} - found {measurements.Count} measurements");
            }
            else
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(jsonString);
                var exceptionString = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                Console.WriteLine($"Measurement request failed, exception: {exceptionString}");
            }
        }

        public async Task GetDynamicLineRatingsForLine(LineDto line, DLRType dlrType = DLRType.HP)
        {
            Console.WriteLine($"\nRequesting DLR for line: {line.Name}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            // By including the optional lineSpanName
            var url = "api/beta/dlr/aggregated-dlr?" +
                      $"fromDateTime={fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"dlrType={dlrType}&" +
                      $"lineName={line.Name}";

            Console.WriteLine($"Sending request to {heimdallClient.BaseAddress}{url}");

            var response = await heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response body: {jsonString}");

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<ApiResponse<DynamicLineRatingDto>>(jsonString);

                foreach (var dynamicLineRating in measurementResponse.Data)
                {
                    Console.WriteLine($"Dynamic line rating at {DateTime.Parse(dynamicLineRating.IntervalStartTime)}: {dynamicLineRating.Ampacity} A\n");
                }
                Console.WriteLine($"DLR response: {measurementResponse.Message} - found {measurementResponse.Data.Count} DLRs");
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
