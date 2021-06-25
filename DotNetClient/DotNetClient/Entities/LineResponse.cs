using System.Collections.Generic;

namespace DotNetClient.Entities
{
    public class LineResponse
    {
        public int code;
        public string message;
        public List<LineDto> data;
    }
}
