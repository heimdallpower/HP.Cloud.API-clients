using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class LineDto
    {
        public string name;
        public string owner;
        public List<LineSpanDto> lineSpans;
    }
}
