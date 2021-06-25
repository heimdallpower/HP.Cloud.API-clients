using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class AggregatedMeasurementsResponse
    {
        public int code;
        public List<AggregatedMeasurementDto> data = new List<AggregatedMeasurementDto>();
        public string message;
    }
}
