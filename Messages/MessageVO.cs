using EmailProcessor.Data.ValueObjects;
using EmailProcessor.MessageBus;
using System;
using System.Text.Json.Serialization;

namespace EmailProcessor.Messages
{
    public class MessageVO : BaseMessage
    {
        public InvoiceVO invoice { get; set; }
    }
}
