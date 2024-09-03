using System.Collections.Generic;

public class Message
{
    public string SongID { get; set; }
    public List<int> UserRatings { get; set; }
    public int SocialMediaShareIncrement { get; set; }
    public int PlayCountIncrement { get; set; }

    public Message(string songID, List<int> userRatings, int socialMediaShareIncrement, int playCountIncrement)
    {
        SongID = songID;
        UserRatings = userRatings;
        SocialMediaShareIncrement = socialMediaShareIncrement;
        PlayCountIncrement = playCountIncrement;
    }
}