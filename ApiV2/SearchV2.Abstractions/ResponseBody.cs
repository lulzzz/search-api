using System.Collections.Generic;

namespace SearchV2.Abstractions
{
    public class ResponseBody
    {
        public IEnumerable<object> Data { get; set; }
        public int? Count { get; set; }
    }
}
