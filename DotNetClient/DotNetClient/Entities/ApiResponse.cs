using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class ApiResponse<T>
    {
        public List<T> Data = new List<T>();
        public string Message;
    }
}
