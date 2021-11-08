using System;
using System.Linq;
using System.Threading.Tasks;
using HeimdallPower;
using HeimdallPower.Enums;
using Newtonsoft.Json;

namespace DotNetClient
{
    public class Program
    {
        private const string ClientId = "INSERT_VARIABLE_HERE";
        private const string PfxCertificatePath = "INSERT_VARIABLE_HERE";
        private const string CertificatePassword = "INSERT_VARIABLE_HERE";
        private const bool UseDeveloperApi = true;

        private static async Task Main(string[] args)
        {
            if (!(args.Length == 0 || args.Length == 4))
            {
                Console.WriteLine("Program only supports 0 or 4 arguments, argument order is: ClientId UseDeveloperApi PfxCertificatePath CertificatePassword");
                return;
            }

            CloudApiClient cloudApiClient;
            if (args.Length == 4)
            {
                var useDeveloperApi = bool.Parse(args[1]);
                cloudApiClient = new CloudApiClient(args[0], useDeveloperApi, args[2], args[3]);
            }
            else
            {
                cloudApiClient = new CloudApiClient(ClientId, UseDeveloperApi, PfxCertificatePath, CertificatePassword);
            }

            Console.WriteLine("Hello Heimdall!\n");

            var lines = await cloudApiClient.GetLines();
            Console.WriteLine($"\n{JsonConvert.SerializeObject(lines, Formatting.Indented)}\n\nRequest data with the ids of lines, spans, and span phases from the response above\n");

            if (!lines.Any())
                Console.WriteLine("Didn't find any lines");

            var toDate = DateTime.UtcNow;
            var fromDate = DateTime.UtcNow.AddDays(-3);
            foreach (var line in lines)
            {
                var measurements = await cloudApiClient.GetAggregatedMeasurements(line, null, fromDate, toDate, IntervalDuration.EveryDay, MeasurementType.Current, AggregationType.Average);
                Console.WriteLine($"\nMeasurements for {line.Name}\n{JsonConvert.SerializeObject(measurements, Formatting.Indented)}");

                var dynamicLineRatings = await cloudApiClient.GetAggregatedDlr(line, fromDate, toDate, DLRType.Cigre, IntervalDuration.EveryDay);
                Console.WriteLine($"\nDynamic line ratings for {line.Name}\n{JsonConvert.SerializeObject(dynamicLineRatings, Formatting.Indented)}");
            }
        }
    }
}