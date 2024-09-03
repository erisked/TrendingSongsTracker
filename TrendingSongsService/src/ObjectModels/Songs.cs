namespace TrendingSongsService.ObjectModel;
public class Songs
{
    public ulong SongId { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public string Genre { get; set; }
    public int PlayCount { get; set; }
    public double UserRating { get; set; }
    public int SocialMediaShares { get; set; }
    public string GeoPopularity { get; set; }
    public DateTime TimestampOfLastPlay { get; set; }
}
