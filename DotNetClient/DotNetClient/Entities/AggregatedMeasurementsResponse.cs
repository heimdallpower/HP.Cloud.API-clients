using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class AggregatedMeasurementsResponse
    {
        public List<AggregatedMeasurementDto> data = new List<AggregatedMeasurementDto>();
        public string message;
    }
}
