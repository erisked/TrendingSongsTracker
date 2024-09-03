// See https://aka.ms/new-console-template for more information
using Confluent.Kafka;

Console.WriteLine("This program just creates the events for songs of a particular genre and pushes them to a kafka queue.");
var messageGenerator = new MessagePusher();
messageGenerator.ProduceMessageStream("test-topic");