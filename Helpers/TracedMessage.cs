using System;
namespace Helpers
{
    public class TracedMessage<T>
    {
        public T Payload { get; set; }
        public Dictionary<string, string> TracingHeaders { get; set; } = new Dictionary<string, string>();
    }

}

