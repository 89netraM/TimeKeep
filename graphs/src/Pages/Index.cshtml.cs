using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TimeKeep.Graphs.Svg;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Graphs.Pages;

public class IndexModel : PageModel
{
	private static readonly IEnumerable<string> KnownCategories = new[]
	{
		"master-project",
		"thesis-writing",
		"presentation",
		"planning-report",
		"opposition",
		"supervisor-meeting",
		"writing-seminar",
	};

	private readonly EntriesServiceClient entriesService;

#nullable disable
	public SvgDocument TimeByLocation { get; private set;}
	public SvgDocument TimeByCategory { get; private set;}
	public SvgDocument TimeByWeek { get; private set;}
	public SvgDocument TimeByDayOfWeek { get; private set;}
	public SvgDocument TimeByHourOfDay { get; private set;}
#nullable restore

	public IndexModel(EntriesServiceClient entriesService)
	{
		this.entriesService = entriesService;
	}

	public async Task OnGetAsync()
	{
		var entries = await FetchEntries();

		var byLocation = TranslateLocations(entries.TimeByLocation());
		TimeByLocation = SvgPie.WritePie(byLocation.OrderByDescending(kvp => kvp.Value));
		var byCategory = TranslateCategories(entries.TimeByCategory(KnownCategories));
		TimeByCategory = SvgPie.WritePie(byCategory.OrderByDescending(kvp => kvp.Value));

		var timePeriods = entries.ToTimePeriods().ToArray();

		var byWeek = timePeriods.TimeByWeek();
		TimeByWeek = SvgGraph.WriteGraph(byWeek.OrderBy(kvp => kvp.Key.WeekNumber), false);
		var byDayOfWeek = TranslateWeekDays(timePeriods.TimeByDayOfWeek());
		TimeByDayOfWeek = SvgGraph.WriteGraph(byDayOfWeek, true);
		var byHourOfDay = timePeriods.TimeByHourOfDay();
		TimeByHourOfDay = SvgGraph.WriteGraph(byHourOfDay, true);
	}

	private ValueTask<Entry[]> FetchEntries()
	{
		var request = new ListRequest();
		request.Categories.Add("master-thesis");
		var response = entriesService.List(request);
		return response.ResponseStream.ReadAllAsync().ToArrayAsync();
	}

	private static IDictionary<string, TimeSpan> TranslateLocations(IDictionary<string, TimeSpan> timeByLocation) =>
		timeByLocation.ToDictionary(kvp => TryTranslate(LocationNames, kvp.Key), kvp => kvp.Value);

	private static readonly IReadOnlyDictionary<string, string> LocationNames = new Dictionary<string, string>
	{
		["masters-office"] = "Master Kontoret",
		["unknown"] = "Okänt",
		["home"] = "Hemma",
		["cygni"] = "Cygni",
	};

	private static IDictionary<string, TimeSpan> TranslateCategories(IDictionary<string, TimeSpan> timeByCategory) =>
		timeByCategory.ToDictionary(kvp => TryTranslate(CategoryNames, kvp.Key), kvp => kvp.Value);

	private static readonly IReadOnlyDictionary<string, string> CategoryNames = new Dictionary<string, string>
	{
		["master-project"] = "Projekt",
		["thesis-writing"] = "Rapport",
		["presentation"] = "Presentation",
		["writing-seminar"] = "Writing Seminar",
		["other"] = "Annat",
		["planning-report"] = "Planerings Rapport",
		["opposition"] = "Opponering",
		["supervisor-meeting"] = "Handledarmöte",
	};

	private static IDictionary<string, TimeSpan> TranslateWeekDays(IDictionary<DayOfWeek, TimeSpan> timeByDayOfWeek) =>
		timeByDayOfWeek.ToDictionary(kvp => TryTranslate(DayOfWeekName, kvp.Key), kvp => kvp.Value);

	private static readonly IReadOnlyDictionary<DayOfWeek, string> DayOfWeekName = new Dictionary<DayOfWeek, string>
	{
		[DayOfWeek.Monday] = "Måndag",
		[DayOfWeek.Tuesday] = "Tisdag",
		[DayOfWeek.Wednesday] = "Onsdag",
		[DayOfWeek.Thursday] = "Torsdag",
		[DayOfWeek.Friday] = "Fredag",
		[DayOfWeek.Saturday] = "Lördag",
		[DayOfWeek.Sunday] = "Söndag",
	};

	private static string TryTranslate<T>(IReadOnlyDictionary<T, string> dictionary, T key) =>
		dictionary.TryGetValue(key, out var value) ? value : key?.ToString() ?? "null";
}
