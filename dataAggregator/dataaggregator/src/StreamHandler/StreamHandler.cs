using Confluent.Kafka;
using TrendingSongsService.DataAggregator;

class StreamHandler
{
    ConsumerConfig config;
    string genre;
    MessagesHandlerNotifier notifier;
    
    public StreamHandler(MessagesHandlerNotifier n)
    {
        config = new ConsumerConfig
        { 
            BootstrapServers = "localhost:9092", 
            GroupId = "consumerGroup1",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        genre = shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.genre);
        notifier = n;
    }

    public void StartStreamReader()
    {
        Thread newThread = new Thread(new ThreadStart(ReadStreamData));
        // Start the thread
        newThread.Start();
    }

    private void ReadStreamData()
    {
        using (var c = new ConsumerBuilder<Null, string>(config).Build())
        {
            c.Subscribe("test-topic");
            while(true)
            {
                try
                {
                    var dr =  c.Consume(timeout: TimeSpan.FromSeconds(1));
                    if(dr!=null)
                    {
                        notifier.NotifyHandlers(dr.Message);
                        Console.WriteLine($"Delivered '{dr.Message.Value}' to '{dr.TopicPartitionOffset}'");
                    }
                }
                catch (ProduceException<Null, string> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }
    
}