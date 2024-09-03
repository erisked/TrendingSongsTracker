using  TrendingSongsService.Requesthandler.Interfaces;
using  System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

public class HttpRequestHandler : IRequestHandler
{
    private readonly IMemoryCache cache;

    public HttpRequestHandler(IMemoryCache ch)
    {
        cache = ch;
    }
    public async Task<string> List(HttpRequest request)
    {
        using var reader = new StreamReader(request.Body);
        var data = await reader.ReadToEndAsync();
        TrendingSongsRequest songRequest = null;
        if (data != null)
        {
            // using the request json itself as the cache key.
            // todo: key should be better, what if order of fields is different???
            // todo: maybe, can use redis?
            if (!cache.TryGetValue(data, out string cachedData))
            {
                // fetch the list from the data processing service.
                Console.WriteLine("Cache miss, feting data from backend");
                // Cache the data for 5 minutes
                songRequest = JsonSerializer.Deserialize<TrendingSongsRequest>(data);

                var songList = 
                cache.Set(data, data, TimeSpan.FromMinutes(5));
                cachedData = data;
            }            
        }
        if(songRequest != null)
        {
            return $"Deserialized, Genre:{songRequest.genre}";
        }
        return $"Data received: {data}";
    }
}

