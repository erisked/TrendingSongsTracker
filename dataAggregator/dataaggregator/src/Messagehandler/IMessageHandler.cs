using Confluent.Kafka;

interface IMessageHandler
{
    void Handle(Message message);
}