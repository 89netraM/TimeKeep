using System;
using System.Linq;
using Grpc.Core;
using TimeKeep.Graphs;
using TimeKeep.RPC.Entries;

var entryService = ServiceFactory.EntryService.Value;

var request = new ListRequest();
request.Categories.Add("master-thesis");
var response = entryService.List(request);
var entries = await response.ResponseStream.ReadAllAsync().ToArrayAsync();

Console.WriteLine();
Console.WriteLine("Time by location:");
var timeByLocation = entries.TimeByLocation();
Console.WriteLine(String.Join("\n", timeByLocation.OrderByDescending(t => t.Value).Select(t => $"{t.Key,-15}{t.Value,11:d\\.hh\\:mm\\:ss}")));

Console.WriteLine();
var timeByCategory = entries.TimeByCategory(new[] { "master-project", "thesis-writing", "presentation", "planning-report", "opposition", "supervisor-meeting", "writing-seminar" });
var totalTime = timeByCategory.Values.Aggregate(TimeSpan.Zero, (t, s) => t + s);
Console.WriteLine($"Time by category: ({totalTime:d\\.hh\\:mm\\:ss})");
Console.WriteLine(String.Join("\n", timeByCategory.OrderByDescending(t => t.Value).Select(t => $"{t.Key,-19}{t.Value,11:d\\.hh\\:mm\\:ss}")));
