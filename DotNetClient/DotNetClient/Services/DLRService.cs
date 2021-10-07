using System;
using System.Threading.Tasks;
using DotNetClient.Entities;
using DotNetClient.Enums;
using Newtonsoft.Json;

namespace DotNetClient.Services
{
    public partial class HeimdallService
    {
        private const string AggregatedDLREndpoint = "api/beta/dlr/aggregated-dlr";

        public async Task GetDynamicLineRatingsForLine(LineDto line, DLRType dlrType = DLRType.HP)
        {
            Console.WriteLine($"\nRequesting dynamic line ratings for line: {line.Name}");

            // By including the optional lineSpanName
            var url = $"{AggregatedDLREndpoint}?" +
                      $"fromDateTime={_fromDate.ToString(DateFormat)}&" +
                      $"toDateTime={_toDate.ToString(DateFormat)}&" +
                      $"intervalDuration={IntervalDuration.EveryDay}&" +
                      $"dlrType={dlrType}&" +
                      $"lineName={line.Name}";

            var response = await _heimdallClient.GetAsync(url);

            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var dlrResponse = JsonConvert.DeserializeObject<ApiResponse<DynamicLineRatingDto>>(jsonString);
                Console.WriteLine($"{dlrResponse.Message} - found {dlrResponse.Data.Count} DLRs");
                Console.WriteLine($"{jsonString}\n");
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
