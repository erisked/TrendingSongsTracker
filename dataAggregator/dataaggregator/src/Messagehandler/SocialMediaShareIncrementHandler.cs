using Confluent.Kafka;
using TrendingSongsService.DataAggregator;

class SocialMediaShareIncrementHander : IMessageHandler, IDataSource
{   
    private Queue<CountMinSketch> batchCMSQueue;
    private CountMinSketch currShareCountTrackerCMS;
    private CountMinSketch runningShareCountTrackerCMS;
    private double mediaShareCountWeight;

    Dictionary<string, List<SongCMSMappingInfo>> liveEntryContributionMapper;

    private DataAggregator dataAggregator;
    private int identifierRegisteredWithDataAggregator;

    public SocialMediaShareIncrementHander()
    {
        dataAggregator = DataAggregator.GetDataAggregatorSingleton();
        identifierRegisteredWithDataAggregator =dataAggregator.RegisterDataSources(this);
        currShareCountTrackerCMS = new CountMinSketch();
        batchCMSQueue = new Queue<CountMinSketch>();
        runningShareCountTrackerCMS = new CountMinSketch();
        mediaShareCountWeight = int.Parse(shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.mediaShareCountWeight));
        liveEntryContributionMapper = new Dictionary<string, List<SongCMSMappingInfo>>();
    }
    public void Handle(Message message)
    {
        if(message.PlayCountIncrement != 0)
        {
            var prevCount = currShareCountTrackerCMS.GetCount(message.SongID);
            currShareCountTrackerCMS.Add(message.SongID, message.SocialMediaShareIncrement);
            runningShareCountTrackerCMS.Add(message.SongID, message.SocialMediaShareIncrement);
            var increment = mediaShareCountWeight*(prevCount + message.SocialMediaShareIncrement);
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
                    cmsCreationTime = currShareCountTrackerCMS.GetCreationTime(),
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
        batchCMSQueue.Enqueue(currShareCountTrackerCMS);
        currShareCountTrackerCMS = new CountMinSketch();
        if(batchCMSQueue.Count == maxQueuelength)
        {
            // cms to be removed on flush.
            var flushedCms = batchCMSQueue.Dequeue();
            // Clear out entries from running cms becasue of outdated CMS.
            runningShareCountTrackerCMS.RemoveCMS(flushedCms);
            // Update aggregator heap with this impact.

            List<SongCMSMappingInfo> impactedEntries = new List<SongCMSMappingInfo>();
            var flushedCmsTime = flushedCms.GetCreationTime();
            foreach(var cmsList in liveEntryContributionMapper.Values)
            {
                if(cmsList.Count()>0)
                {
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