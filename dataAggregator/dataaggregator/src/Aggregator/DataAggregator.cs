using TrendingSongsService.DataAggregator;

class DataAggregator
{

    private static DataAggregator? dataAggregator;
    private MinHeap<double> AggregatedDataHeap;

    private int trendTimeInMinutes = 0;
    private int batchFlushTimeInMinutes = 0;

    private int identifierCount = 0;

    Dictionary<int, IDataSource> messageHandlers;

    // CREATE A SINGLETON!!
    public static DataAggregator GetDataAggregatorSingleton()
    {
        if(dataAggregator==null)
        {
            dataAggregator = new DataAggregator(
            shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.trendTimePeriod),
            shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.batchFlushTime));
        }
        return dataAggregator;
    }
    private DataAggregator(string aggregationTime, string batchFlushTime)
    {
        AggregatedDataHeap = new MinHeap<double>(100);
        messageHandlers = new Dictionary<int, IDataSource>();
        trendTimeInMinutes = int.Parse(aggregationTime);
        batchFlushTimeInMinutes = int.Parse(batchFlushTime);
        StartFlushThread();
    }

    public List<string> GetAggregatedList()
    {
        return AggregatedDataHeap.GetSortedList();
    }
    public void Update(string songID, double increment, int MessageHandlerIdentifier)
    {
        var evicted = AggregatedDataHeap.Update(songID, increment);
        sendEvictionUpdates(evicted);

    }

    private void sendEvictionUpdates(string evicted)
    {
        foreach(var handler in messageHandlers.Values)
        {
            handler.SyncLiveEntries(evicted);
        }
    }

    public void CleanUpHeap(List<SongCMSMappingInfo> imapctedValues)
    {
        foreach(var cmsMappingInfo in imapctedValues)
        {
            // the CMS that are phased out - their contribution to the heap would be removed.
            // no need to remove the entries from other datasources? 
            AggregatedDataHeap.Update(cmsMappingInfo.songsId, -1*cmsMappingInfo.cmsContribution);
        }
    }
    public int RegisterDataSources(IDataSource dataSource)
    {
        identifierCount ++;
        messageHandlers.Add(identifierCount, dataSource);
        return identifierCount;
    }
    private void StartFlushThread()
    {
        Thread newThread = new Thread(new ThreadStart(FlushAllDataSources));
        
        // Start the thread
        newThread.Start();
    }
    private void FlushAllDataSources()
    {
        // Set up a timer to call Flush every 30 minutes
        var _timer = new Timer(flushAll, null, TimeSpan.Zero, TimeSpan.FromMinutes(batchFlushTimeInMinutes));

        // The Timer will handle calling flushAll every 30 minutes
        Console.WriteLine("Timer set up to call flushAll every 30 minutes.");
    }
    private void flushAll(object state)
    {
        int maxQueuelength = trendTimeInMinutes/batchFlushTimeInMinutes;
        foreach(var handler in messageHandlers.Values.ToList())
        {
            handler.Flush(maxQueuelength);
        }
    }

}