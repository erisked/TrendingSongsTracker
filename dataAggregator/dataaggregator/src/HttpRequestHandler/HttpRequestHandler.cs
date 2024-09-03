using  TrendingSongsService.Requesthandler.Interfaces;
using  System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;

public class HttpRequestHandler : IRequestHandler
{
    DataAggregator dataAggregatorService;
    public HttpRequestHandler()
    {
        dataAggregatorService = DataAggregator.GetDataAggregatorSingleton();
    }
    public async Task<List<string>> List(HttpRequest request)
    {   
        return await Task.Run(() => dataAggregatorService.GetAggregatedList());
    }
}

