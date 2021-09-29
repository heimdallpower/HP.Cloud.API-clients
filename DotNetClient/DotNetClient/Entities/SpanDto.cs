using System;
using System.Collections.Generic;

namespace DotNetClient.Entities
{
    public class SpanDto
    {
        public Guid Id { get; set; }
        public IEnumerable<SpanPhaseDto> SpanPhases { get; set; }
    }
}
