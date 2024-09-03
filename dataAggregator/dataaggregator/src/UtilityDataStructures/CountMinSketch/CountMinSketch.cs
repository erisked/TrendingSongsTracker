using System.ComponentModel;
using System.Text;
using K4os.Hash.xxHash;

class CountMinSketch
{

    /*
    Need to take sufficient depth such that over a million unique song's count can live in CMS with 99% accuracy. 
    Using chatGPT for calculation -
    Width (w): 2048
    Depth (d): 32

    The error rate (ε) is calculated as:
    ϵ=we​≈20482.718​≈0.00133
    Using the formula for the number of unique events (n):
    n≈ϵw​
    Substituting the values:
    n≈0.001332048​≈1,540,601
    So, with a width of 2048 and a depth of 32, you can store approximately 1,540,601 unique events with a 99% probability of accuracy12.
    */

    // lenght of CMS.
    private int length = 2048;

    // Width of CMS - corresponds to the number of hash functions that we have.
    private int width = 32;

    // Time at which the cmS was created. 
    private DateTime creationTime;

    public int[,] CMS;

    private List<string> HashSalts;

    // In case we choose to update the CMS from multiple threads.
    private readonly object writelock = new object();

    // using static salts becasue we want to ensure that the hash is same for strings across CMSes.
    private List<string> getSalts()
    {
        return new List<string>
        {
            "e3b0c44298fc1c14",
            "5d41402abc4b2a76",
            "6dcd4ce23d88e2ee",
            "8e296a067a375633",
            "9e107d9d372bb682",
            "b1946ac92492d234",
            "c3fcd3d76192e400",
            "d41d8cd98f00b204",
            "e4da3b7fbbce2345",
            "f96b697d7cb7938d",
            "0cc175b9c0f1b6a8",
            "1f0e3dad99908345",
            "2e7d2c03a9507ae2",
            "3c59dc048e885024",
            "4a8a08f09d37b737",
            "5d41402abc4b2a76",
            "6dcd4ce23d88e2ee",
            "7e296a067a375633",
            "8e107d9d372bb682",
            "9e1946ac92492d23",
            "a3fcd3d76192e400",
            "b41d8cd98f00b204",
            "c4da3b7fbbce2345",
            "d96b697d7cb7938d",
            "e0cc175b9c0f1b6a",
            "f1f0e3dad9990834",
            "0e7d2c03a9507ae2",
            "1c59dc048e885024",
            "2a8a08f09d37b737",
            "3d41402abc4b2a76",
            "4dcd4ce23d88e2ee",
            "5e296a067a375633"
        };
    }

    public CountMinSketch()
    {
        // rows = width, columns = lenght
        CMS = new int[width, length];
        for(int i=0; i<width;i++)
        {
            for(int j = 0; j < length;j++)
            {
                CMS[i,j] = 0;
            }
        }
        creationTime = DateTime.Now;
        HashSalts = getSalts();
    }


    public void Add(string key, int increment = 1)
    {
        lock (writelock)
        {
            for (int i = 0; i < HashSalts.Count; i++)
            {
                string saltedKey = key + HashSalts[i];
                byte[] bytes = Encoding.UTF8.GetBytes(saltedKey);
        
                ulong hash = XXH64.DigestOf(bytes);
                int index = (int)(hash % 2048);
                CMS[i, index] += increment;
            }
        }
    }

    // A cms that spans for 2 hrs. Can be used to predict the usage for second hr, if we remove the CMS for first hour.
    // This is to get the running count of the songs.
    // eg - cms(7AM-1PM) = cms(6AM-1PM) - cms(6AM-7PM) 
    public void RemoveCMS(CountMinSketch oldCms)
    {
        lock (writelock)
        {
            for(int i=0; i<width;i++)
            {
                for(int j = 0; j < length;j++)
                {
                    CMS[i,j] = CMS[i,j]-oldCms.CMS[i,j];
                }
            }
        }
    }

    // can return the correct count with ~99% accuracy corresponding to this key for upto 1.5 million unique keys.
    public int GetCount(string key)
    {
        int min = int.MaxValue;
        for (int i = 0; i < HashSalts.Count; i++)
        {
            string saltedKey = key + HashSalts[i];
            byte[] bytes = Encoding.UTF8.GetBytes(saltedKey);
            
            ulong hash = XXH64.DigestOf(bytes);
            int index = (int)(hash % 2048);
            min = (CMS[i,index]<min)?CMS[i,index]:min;
        }
        return min;
    }

    public DateTime GetCreationTime()
    {
        return creationTime;
    }
}