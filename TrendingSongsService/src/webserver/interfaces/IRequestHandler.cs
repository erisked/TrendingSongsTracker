namespace TrendingSongsService.Requesthandler.Interfaces
{
    public interface IRequestHandler
    {
        Task<string> List(HttpRequest request);
    }
}