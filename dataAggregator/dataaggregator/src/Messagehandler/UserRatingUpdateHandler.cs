using Confluent.Kafka;
using TrendingSongsService.DataAggregator;

class UserRatingUpdateHander : IMessageHandler, IDataSource
{
    // private CountMinSketch totalUserRatingTrackerCMS = null;
    // private CountMinSketch userRatingCountCMS = null;
    // private MinHeap<double> averageUserRatingHeap;

    private Queue<CountMinSketch> batchCMSQueue_UserRatingCount;
    private CountMinSketch currTotalUserRatingCMS;
    private CountMinSketch runningTotalUserRatingCMS;
    private Queue<CountMinSketch> batchCMSQueue_TotalUserRating;
    private CountMinSketch currUserRatingCountCMS;
    private CountMinSketch runningUserRatingCountCMS;
    private double userRatingWeight;

    Dictionary<string, List<SongCMSMappingInfo>> liveEntryContributionMapper;


    private DataAggregator dataAggregator;
    private int identifierRegisteredWithDataAggregator;
    public UserRatingUpdateHander()
    {
        dataAggregator = DataAggregator.GetDataAggregatorSingleton();
        identifierRegisteredWithDataAggregator =dataAggregator.RegisterDataSources(this);
        batchCMSQueue_UserRatingCount = new Queue<CountMinSketch>();
        batchCMSQueue_TotalUserRating = new Queue<CountMinSketch>();

        currTotalUserRatingCMS = new CountMinSketch();
        runningTotalUserRatingCMS = new CountMinSketch();

        currUserRatingCountCMS = new CountMinSketch();
        runningUserRatingCountCMS = new CountMinSketch();

        userRatingWeight = int.Parse(shardConfigurationReader.GetShardConfiguration(shardConfigurationReader.userRatingWeight));
        liveEntryContributionMapper = new Dictionary<string, List<SongCMSMappingInfo>>();
    }
    public void Handle(Message message)
    {
        if(message.UserRatings.Count != 0)
        {
            // get the impact of message on the datasource's rating.
            var prevCount = currTotalUserRatingCMS.GetCount(message.SongID);
            var prevTotalRating = currUserRatingCountCMS.GetCount(message.SongID);

            currTotalUserRatingCMS.Add(message.SongID, message.UserRatings.Count);
            int userRatingIncrement = 0;
            foreach(var rating in message.UserRatings)
            {
                userRatingIncrement+=rating;
            }
            currTotalUserRatingCMS.Add(message.SongID, userRatingIncrement);
            int newCount = prevCount + message.UserRatings.Count();
            if(newCount != 0)
            {
                double newAvgUserRating = (double)(prevTotalRating + userRatingIncrement)/((double)(prevCount + message.UserRatings.Count()));
                double prevAvgUserRating = (double)(prevTotalRating)/((double)(prevCount));
                double increment = (double)(newAvgUserRating - prevAvgUserRating)*userRatingWeight;

                // Update the datasource with increment.
                dataAggregator.Update(message.SongID, increment, identifierRegisteredWithDataAggregator);

                // update song-cms map with with song that was added with current CMS.
                if(liveEntryContributionMapper.ContainsKey(message.SongID))
                {
                    var count = liveEntryContributionMapper[message.SongID].Count();
                    liveEntryContributionMapper[message.SongID][count-1].cmsContribution += increment;
                }
                else
                {
                    var CMSinfo = new SongCMSMappingInfo(){
                        songsId = message.SongID,

                        // note: since we use currTotalUserRatingCMS as identifier, we should use the same during dequeue.
                        cmsCreationTime = currTotalUserRatingCMS.GetCreationTime(),
                        cmsContribution = userRatingIncrement,
                    };
                    liveEntryContributionMapper.Add(message.SongID, new List<SongCMSMappingInfo>() {CMSinfo});
                }
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

    public void Flush(int maxQueuelength)
    {
        batchCMSQueue_TotalUserRating.Enqueue(currTotalUserRatingCMS);
        currTotalUserRatingCMS = new CountMinSketch();

        batchCMSQueue_UserRatingCount.Enqueue(currUserRatingCountCMS);
        currUserRatingCountCMS = new CountMinSketch();       

        if(batchCMSQueue_UserRatingCount.Count == maxQueuelength)
        {
            // cms to be removed on flush.
            var flushedCms_UserRatingCount = batchCMSQueue_UserRatingCount.Dequeue();
            // Clear out entries from running cms becasue of outdated CMS.
            runningTotalUserRatingCMS.RemoveCMS(flushedCms_UserRatingCount);

            var flushedCms_TotalUserRating = batchCMSQueue_TotalUserRating.Dequeue();
            runningUserRatingCountCMS.RemoveCMS(flushedCms_TotalUserRating);

            List<SongCMSMappingInfo> impactedEntries = new List<SongCMSMappingInfo>();
            var flushedCmsTime = flushedCms_TotalUserRating.GetCreationTime();
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