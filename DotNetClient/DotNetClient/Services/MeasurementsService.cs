using System;
using System.Threading.Tasks;
using DotNetClient.Entities;
using DotNetClient.Enums;
using Newtonsoft.Json;

namespace DotNetClient.Services
{
    public partial class HeimdallService
    {
        public async Task GetAggregatedMeasurementsForLine(LineDto line)
        {
            Console.WriteLine($"Requesting measurements for line: {line.Name}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            var url = "api/v1/aggregated-measurements?" +
                      $"fromDateTime={_fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={_toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"measurementType={MeasurementType.Current}&" +
                      $"aggregationType={AggregationType.Max}&" +
                      $"lineId={line.Id}";

            Console.WriteLine($"Sending request to {_heimdallClient.BaseAddress}{url}");

            var response = await _heimdallClient.GetAsync(url);

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
    }
}
