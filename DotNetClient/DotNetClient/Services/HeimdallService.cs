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
    public partial class HeimdallService
    {
        private readonly HttpClient _heimdallClient;
        private readonly DateTime _toDate;
        private readonly DateTime _fromDate;
        private const string DateFormat = "o";

        public HeimdallService(HttpClient httpClient)
        {
            _heimdallClient = httpClient;
            _toDate = DateTime.UtcNow;
            _fromDate = DateTime.UtcNow.AddDays(-7); 
        }

        public async Task Run()
        {
            var lines = await GetLines();
            if (!lines.Any())
                Console.WriteLine("Didn't find any lines");
            foreach (var line in lines)
            {
                await GetAggregatedMeasurementsForLine(line);
                await GetDynamicLineRatingsForLine(line);
            }
        }
    }
}
