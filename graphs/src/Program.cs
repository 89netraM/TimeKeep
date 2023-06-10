using System;
using System.Linq;
using System.Threading;
using Grpc.Core;
using TimeKeep.Graphs;
using TimeKeep.Graphs.Svg;
using TimeKeep.RPC.Entries;

var entryService = ServiceFactory.EntryService.Value;

var request = new ListRequest();
request.Categories.Add("master-thesis");
var response = entryService.List(request);
var entries = await response.ResponseStream.ReadAllAsync().ToArrayAsync();

Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

Console.WriteLine();
Console.WriteLine("Time by location:");
var timeByLocation = entries.TimeByLocation();
Console.WriteLine(String.Join("\n", timeByLocation.OrderByDescending(t => t.Value).Select(t => $"{t.Key,-15}{t.Value,11:d\\.hh\\:mm\\:ss}")));
SvgPie.WritePie(timeByLocation.OrderByDescending(t => t.Value), "time-by-location.svg");

Console.WriteLine();
var timeByCategory = entries.TimeByCategory(new[] { "master-project", "thesis-writing", "presentation", "planning-report", "opposition", "supervisor-meeting", "writing-seminar" });
var totalTime = timeByCategory.Values.Aggregate(TimeSpan.Zero, (t, s) => t + s);
Console.WriteLine($"Time by category: ({totalTime:d\\.hh\\:mm\\:ss})");
Console.WriteLine(String.Join("\n", timeByCategory.OrderByDescending(t => t.Value).Select(t => $"{t.Key,-19}{t.Value,11:d\\.hh\\:mm\\:ss}")));
SvgPie.WritePie(timeByCategory.OrderByDescending(t => t.Value), "time-by-category.svg");

var timePeriods = entries.ToTimePeriods().ToArray();

Console.WriteLine();
var timeByDayOfWeek = timePeriods.TimeByDayOfWeek();
Console.WriteLine(String.Join("\n", timeByDayOfWeek.OrderBy(t => Int32.Abs(((int)t.Key + 6) % 7)).Select(t => $"{t.Key,-10}{t.Value,11:d\\.hh\\:mm\\:ss}")));
SvgGraph.WriteGraph(timeByDayOfWeek.OrderBy(t => Int32.Abs(((int)t.Key + 6) % 7)), true, "time-by-day-of-week.svg");

Console.WriteLine();
var timeByWeek = timePeriods.TimeByWeek();
Console.WriteLine(String.Join("\n", timeByWeek.OrderBy(t => t.Key).Select(t => $"{t.Key,-10}{t.Value,11:d\\.hh\\:mm\\:ss}")));
SvgGraph.WriteGraph(timeByWeek.OrderBy(t => t.Key), false, "time-by-week.svg");

Console.WriteLine();
var timeByHourOfDay = timePeriods.TimeByHourOfDay();
Console.WriteLine(String.Join("\n", timeByHourOfDay.OrderBy(t => t.Key).Select(t => $"{t.Key,-10}{t.Value,11:d\\.hh\\:mm\\:ss}")));
SvgGraph.WriteGraph(timeByHourOfDay.OrderBy(t => t.Key), true, "time-by-hour-of-day.svg");
