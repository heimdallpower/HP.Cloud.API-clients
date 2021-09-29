﻿using System;
using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class LineDto
    {
        public Guid Id { get; set; }
        public string Name;
        public string Owner;
        public List<SpanDto> Spans;
    }
}
