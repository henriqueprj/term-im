using System;

namespace TermIM.Contracts
{
    public class Message
    {
        public DateTime Timestamp { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}
