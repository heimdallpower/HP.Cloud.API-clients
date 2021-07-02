using System.Collections.Generic;
namespace DotNetClient.Entities
{
    public class ApiResponse<T>
    {
        public List<T> data = new List<T>();
        public string message;
    }
}
