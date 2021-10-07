using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DotNetClient.Entities
{
    public class AggregatedResponse<T> where T : class
    {
        public Guid Id { get; set; }
        public IEnumerable<T> Data { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Owner { get; set; }
    }
}
