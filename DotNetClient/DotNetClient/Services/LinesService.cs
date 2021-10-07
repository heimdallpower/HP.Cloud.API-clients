using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetClient.Entities;
using Newtonsoft.Json;

namespace DotNetClient.Services
{
    public partial class HeimdallService
    {
        private const string LinesEndpoint = "api/v1/lines";

        public async Task<List<LineDto>> GetLines()
        {
            var response = await _heimdallClient.GetAsync(LinesEndpoint);
            var jsonString = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var linesResponse = JsonConvert.DeserializeObject<LineResponse>(jsonString);
                Console.WriteLine($"\nUse ids from this response to request Data\n{JsonConvert.SerializeObject(linesResponse.Data, Formatting.Indented)}\n");
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
    }
}
