class SongSimulationData
{
    private static List<string> songNames = new List<string>
    {
        "Song 1", "Song 2", "Song 3", "Song 4", "Song 5", "Song 6", "Song 7", "Song 8", "Song 9", "Song 10",
        "Song 11", "Song 12", "Song 13", "Song 14", "Song 15", "Song 16", "Song 17", "Song 18", "Song 19", "Song 20",
        "Song 21", "Song 22", "Song 23", "Song 24", "Song 25", "Song 26", "Song 27", "Song 28", "Song 29", "Song 30",
        "Song 31", "Song 32", "Song 33", "Song 34", "Song 35", "Song 36", "Song 37", "Song 38", "Song 39", "Song 40",
        "Song 41", "Song 42", "Song 43", "Song 44", "Song 45", "Song 46", "Song 47", "Song 48", "Song 49", "Song 50",
        "Song 51", "Song 52", "Song 53", "Song 54", "Song 55", "Song 56", "Song 57", "Song 58", "Song 59", "Song 60",
        "Song 61", "Song 62", "Song 63", "Song 64", "Song 65", "Song 66", "Song 67", "Song 68", "Song 69", "Song 70",
        "Song 71", "Song 72", "Song 73", "Song 74", "Song 75", "Song 76", "Song 77", "Song 78", "Song 79", "Song 80",
        "Song 81", "Song 82", "Song 83", "Song 84", "Song 85", "Song 86", "Song 87", "Song 88", "Song 89", "Song 90",
        "Song 91", "Song 92", "Song 93", "Song 94", "Song 95", "Song 96", "Song 97", "Song 98", "Song 99", "Song 100",
        "Song 101", "Song 102", "Song 103", "Song 104", "Song 105", "Song 106", "Song 107", "Song 108", "Song 109", "Song 110",
        "Song 111", "Song 112", "Song 113", "Song 114", "Song 115", "Song 116", "Song 117", "Song 118", "Song 119", "Song 120",
        "Song 121", "Song 122", "Song 123", "Song 124", "Song 125", "Song 126", "Song 127", "Song 128", "Song 129", "Song 130",
        "Song 131", "Song 132", "Song 133", "Song 134", "Song 135", "Song 136", "Song 137", "Song 138", "Song 139", "Song 140",
        "Song 141", "Song 142", "Song 143", "Song 144", "Song 145", "Song 146", "Song 147", "Song 148", "Song 149", "Song 150",
        "Song 151", "Song 152", "Song 153", "Song 154", "Song 155", "Song 156", "Song 157", "Song 158", "Song 159", "Song 160",
        "Song 161", "Song 162", "Song 163", "Song 164", "Song 165", "Song 166", "Song 167", "Song 168", "Song 169", "Song 170",
        "Song 171", "Song 172", "Song 173", "Song 174", "Song 175", "Song 176", "Song 177", "Song 178", "Song 179", "Song 180",
        "Song 181", "Song 182", "Song 183", "Song 184", "Song 185", "Song 186", "Song 187", "Song 188", "Song 189", "Song 190",
        "Song 191", "Song 192", "Song 193", "Song 194", "Song 195", "Song 196", "Song 197", "Song 198", "Song 199", "Song 200"
    };

    private static Random random = new Random();

    public static Message GetRandomMessage()
    {
        int songIndex = random.Next(1, songNames.Count);
        List<int> userRatings = new List<int> { random.Next(0, 6) };
        int socialMediaShareIncrement = random.Next(0, 3);
        int playCountIncrement = random.Next(1, 6);

        return new Message(songNames[songIndex], userRatings, socialMediaShareIncrement, playCountIncrement);
    }
}