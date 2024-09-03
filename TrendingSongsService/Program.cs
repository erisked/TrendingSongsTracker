using TrendingSongsService.Requesthandler.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add local cache.
builder.Services.AddMemoryCache();
// Add Http request handler.
builder.Services.AddSingleton<IRequestHandler, HttpRequestHandler>();
var app = builder.Build();

var handler = app.Services.GetRequiredService<IRequestHandler>();

app.MapGet("/about", () => "Hi, this is a service to fetch you the list of 100 trending songs. Hit endpoint 'api/trending-songs' to get the list");
app.MapGet("/api/trending-songs", async (HttpRequest request) => await handler.List(request));
app.Run();