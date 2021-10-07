using System;
using System.Threading.Tasks;
using DotNetClient.Entities;
using DotNetClient.Enums;
using Newtonsoft.Json;

namespace DotNetClient.Services
{
    public partial class HeimdallService
    {
        private const string AggregatedMeasurementsEndpoint = "api/v1/aggregated-measurements";

        public async Task GetAggregatedMeasurementsForLine(LineDto line)
        {
            Console.WriteLine($"Requesting measurements for line: {line.Name}");

            var url = $"{AggregatedMeasurementsEndpoint}?" +
                      $"fromDateTime={_fromDate.ToString(DateFormat)}&" +
                      $"toDateTime={_toDate.ToString(DateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"measurementType={MeasurementType.Current}&" +
                      $"aggregationType={AggregationType.Max}&" +
                      $"lineId={line.Id}";


            var response = await _heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var measurementResponse = JsonConvert.DeserializeObject<ApiResponse<AggregatedFloatValueDto>>(jsonString);
                var measurements = measurementResponse.Data;
                Console.WriteLine($"{measurementResponse.Message} - found {measurements.Count} measurements");
                Console.WriteLine(jsonString);
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
