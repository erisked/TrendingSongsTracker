using System;

public class Song
{
    public int SongID { get; set; }
    public string Title { get; set; }
    public string Artist { get; set; }
    public string Album { get; set; }
    public string Genre { get; set; }
    public int PlayCount { get; set; }
    public double UserRating { get; set; }
    public int SocialMediaShares { get; set; }
    public string GeographicPopularity { get; set; }
    public DateTime LastPlayTimestamp { get; set; }

    public Song(int songID, string title, string artist, string album, string genre, int playCount, double userRating, int socialMediaShares, string geographicPopularity, DateTime lastPlayTimestamp)
    {
        SongID = songID;
        Title = title;
        Artist = artist;
        Album = album;
        Genre = genre;
        PlayCount = playCount;
        UserRating = userRating;
        SocialMediaShares = socialMediaShares;
        GeographicPopularity = geographicPopularity;
        LastPlayTimestamp = lastPlayTimestamp;
    }

    public override string ToString()
    {
        return $"SongID: {SongID}, Title: {Title}, Artist: {Artist}, Album: {Album}, Genre: {Genre}, PlayCount: {PlayCount}, UserRating: {UserRating}, SocialMediaShares: {SocialMediaShares}, GeographicPopularity: {GeographicPopularity}, LastPlayTimestamp: {LastPlayTimestamp}";
    }
}