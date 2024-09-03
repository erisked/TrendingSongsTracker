using Microsoft.AspNetCore.Http;

namespace TrendingSongsService.Requesthandler.Interfaces
{
    public interface IRequestHandler
    {
        Task<List<string>> List(HttpRequest request);
    }
}