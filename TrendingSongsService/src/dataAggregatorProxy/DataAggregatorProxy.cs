using TrendingSongsService.ObjectModel;
using Confluent.kafka;
namespace TrendingSongsService.DataAggregator.Proxy;
 
public class DataAggregatorProxy
{
    public List<Songs> GetTopStreamingSongs(TrendingSongsRequest request)
    {
        // todo the call to the aggregator service is made here.
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

        // If serializers are not specified, default serializers from
        // `Confluent.Kafka.Serializers` will be automatically used where
        // available. Note: by default strings are encoded as UTF8.
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = await p.ProduceAsync("myTopicName", new Message<Null, string> { Value="test" });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
        return null;
    }
}
