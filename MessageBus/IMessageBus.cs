using System.Threading.Tasks;

namespace EmailProcessor.MessageBus
{
    public interface IMessageBus
    {
        Task PublicMessage(BaseMessage message, string queueName);
    }
}
