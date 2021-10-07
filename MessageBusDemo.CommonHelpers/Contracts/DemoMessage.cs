using System;

namespace MessageBusDemo.CommonHelpers.Contracts
{
    public class DemoMessage
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
    }
}