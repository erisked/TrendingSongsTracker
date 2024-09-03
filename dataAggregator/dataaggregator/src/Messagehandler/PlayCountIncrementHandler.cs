using Confluent.Kafka;
using TrendingSongsService.DataAggregator;

class playCountIncrementHandler : IMessageHandler, IDataSource
{
    // List of all previous CMS that were committed as batch to the current CMS.
    private Queue<CountMinSketch> batchCMSQueue;
    private CountMinSketch currPlayCountTrackerCMS;
    private CountMinSketch runningPlayCountTrackerCMS;
    private double playCountWeight;

    Dictionary<string, List<SongCMSMappingInfo>> liveEntryContributionMapper;

    private DataAggregator dataAggregator;
    private int identifierRegisteredWithDataAggregator;
    public playCountIncrementHandler()
    {
        dataAggregator = DataAggregator.GetDataAggregatorSingleton();
        identifierRegisteredWithDataAggregator =dataAggregator.RegisterDataSources(this);
        currPlayCountTrackerCMS = new CountMinSketch();
        batchCMSQueue = new Queue<CountMinSketch>();
        runningPlayCountTrackerCMS = new CountMinSketch();
        playCountWeight = int.Parse(shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.playCountWeight));
        liveEntryContributionMapper = new Dictionary<string, List<SongCMSMappingInfo>>();
    }
    public void Handle(Message message)
    {
        if(message.PlayCountIncrement != 0)
        {
            var prevCount = currPlayCountTrackerCMS.GetCount(message.SongID);
            currPlayCountTrackerCMS.Add(message.SongID, message.PlayCountIncrement);
            runningPlayCountTrackerCMS.Add(message.SongID, message.PlayCountIncrement);
            var increment = playCountWeight*(prevCount + message.PlayCountIncrement);
            dataAggregator.Update(message.SongID, increment, identifierRegisteredWithDataAggregator);
            if(liveEntryContributionMapper.ContainsKey(message.SongID))
            {
                var count = liveEntryContributionMapper[message.SongID].Count();
                liveEntryContributionMapper[message.SongID][count-1].cmsContribution += increment;
            }
            else
            {
                var CMSinfo = new SongCMSMappingInfo(){
                    songsId = message.SongID,
                    cmsCreationTime = currPlayCountTrackerCMS.GetCreationTime(),
                    cmsContribution = increment,
                };
                liveEntryContributionMapper.Add(message.SongID, new List<SongCMSMappingInfo>() {CMSinfo});
            }
        }
    }

    public void SyncLiveEntries(string evicted)
    {
        if(liveEntryContributionMapper.ContainsKey(evicted))
        {
            liveEntryContributionMapper.Remove(evicted);
        }
    }

    // persists the CMS of the last batch commit to a queue.
    // And returns the list of relavent songs(songs in the top 100 list) along with the weights from the CMS that is now removed. 
    public void Flush(int maxQueuelength)
    {
        batchCMSQueue.Enqueue(currPlayCountTrackerCMS);
        currPlayCountTrackerCMS = new CountMinSketch();
        if(batchCMSQueue.Count == maxQueuelength)
        {
            // cms to be removed on flush.
            var flushedCms = batchCMSQueue.Dequeue();
            // Clear out entries from running cms becasue of outdated CMS.
            runningPlayCountTrackerCMS.RemoveCMS(flushedCms);
            // Update aggregator heap with this impact.

            List<SongCMSMappingInfo> impactedEntries = new List<SongCMSMappingInfo>();
            var flushedCmsTime = flushedCms.GetCreationTime();
            foreach(var cmsList in liveEntryContributionMapper.Values)
            {
                if(cmsList.Count()>0)
                {
                    // first entry is the oldest entry. It should be the only one to be removed.
                    if(cmsList[0].cmsCreationTime.CompareTo(flushedCmsTime) <= 0)
                    {
                        impactedEntries.Add(cmsList[0]);
                    }
                }
            }
            dataAggregator.CleanUpHeap(impactedEntries);
        }
    }
}