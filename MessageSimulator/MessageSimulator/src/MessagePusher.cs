using System.Text.Json;
using Confluent.Kafka;

class MessagePusher
{
    public void ProduceMessageStream(string topic)
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };
        while(true)
        {
            using (var p = new ProducerBuilder<Null, string>(config).Build())
            {
                try
                {
                    var message = SongSimulationData.GetRandomMessage();
                    string jsonString = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
                    var dr = p.ProduceAsync(topic, new Message<Null, string> { Value=jsonString });
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
}