namespace TrendingSongsService.DataAggregator;

class shardConfigurationReader
{
    public const string region = "region";
    public const string genre = "genre";
    public const string batchFlushTime = "batchFlushTime";
    public const string trendTimePeriod = "trendTimePeriod";
    public const string playCountWeight = "playCountWeight";
    public const string mediaShareCountWeight = "mediaShareCountWeight";
    public const string userRatingWeight = "userRatingWeight";
    private static Dictionary<string, string> config = new Dictionary<string, string>();

    static shardConfigurationReader()
    {
        LoadConfig("ShardConfig");
    }

    private static void LoadConfig(string filePath)
    {
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Split(':');
            if (parts.Length == 2)
            {
                config[parts[0].Trim()] = parts[1].Trim();
            }
        }
    }

    public static string GetShardConfiguration(string key)
    {
        if (config.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }


}