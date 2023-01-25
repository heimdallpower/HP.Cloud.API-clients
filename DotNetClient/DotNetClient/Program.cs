using System;
using System.Linq;
using System.Threading.Tasks;
using HeimdallPower;
using HeimdallPower.Enums;
using Newtonsoft.Json;

string ClientId = "INSERT CLIENT ID HERE";
string ClientSecret = "INSERT CLIENT SECRET HERE";
bool UseDeveloperApi = true;

CloudApiClient cloudApiClient = new(ClientId, ClientSecret, UseDeveloperApi);

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