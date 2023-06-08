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

var totalTime = entries.Aggregate(TimeSpan.Zero, (total, entry) => total + ((entry.End?.ToDateTime() ?? DateTime.UtcNow) - entry.Start.ToDateTime()));
Console.WriteLine($"Total time: {totalTime}");
