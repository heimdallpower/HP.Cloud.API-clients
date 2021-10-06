using System;
using System.Threading.Tasks;
using DotNetClient.Entities;
using DotNetClient.Enums;
using Newtonsoft.Json;

namespace DotNetClient.Services
{
    public partial class HeimdallService
    {
        public async Task GetDynamicLineRatingsForLine(LineDto line, DLRType dlrType = DLRType.HP)
        {
            Console.WriteLine($"\nRequesting DLR for line: {line.Name}");

            var dateFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
            // By including the optional lineSpanName
            var url = "api/beta/dlr/aggregated-dlr?" +
                      $"fromDateTime={_fromDate.ToString(dateFormat)}&" +
                      $"toDateTime={_toDate.ToString(dateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"dlrType={dlrType}&" +
                      $"lineName={line.Name}";

            Console.WriteLine($"Sending request to {_heimdallClient.BaseAddress}{url}");

            var response = await _heimdallClient.GetAsync(url);

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
