// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;
using TrendingSongsService.DataAggregator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

Console.WriteLine("This service aggregates the events for songs of a particular genre. This server is part of a cluster, where each shard corresponds to a unique genre.");

MessagesHandlerNotifier notifier = new MessagesHandlerNotifier();
notifier.RegisterMessageHandler(new playCountIncrementHandler());
notifier.RegisterMessageHandler(new SocialMediaShareIncrementHander());
notifier.RegisterMessageHandler(new UserRatingUpdateHander());

StreamHandler handler = new StreamHandler(notifier);
handler.StartStreamReader();

var builder = WebApplication.CreateBuilder(args);
var httpRequestHandler = new HttpRequestHandler();
var app = builder.Build();

app.MapGet("/dataaggregator/get", async (HttpRequest request) => await httpRequestHandler.List(request));
app.Run();