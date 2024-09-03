using Confluent.Kafka;
using Newtonsoft.Json;
using TrendingSongsService.DataAggregator;

class MessagesHandlerNotifier
{

    List<IMessageHandler> messageHandlers;
    public MessagesHandlerNotifier()
    {
        messageHandlers = new List<IMessageHandler>();
    }

    public void RegisterMessageHandler(IMessageHandler messageHandler)
    {
        messageHandlers.Add(messageHandler);
    }
    public void NotifyHandlers(Message<Confluent.Kafka.Null, string> kafkaMessage)
    {
        if(kafkaMessage != null && kafkaMessage.Value != null)
        {
            try
            {
                Message message = JsonConvert.DeserializeObject<Message>(kafkaMessage.Value);
                foreach(var handler in messageHandlers)
                {
                    handler.Handle(message);
                }
            }
            catch {
                Console.WriteLine("malformed message body can deserialize"+kafkaMessage.Value);
            }
        }
    }

}