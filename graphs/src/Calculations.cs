using System;
using System.Collections.Generic;
using System.Linq;
using TimeKeep.RPC.Entries;

namespace TimeKeep.Graphs;

public static class Calculations
{
	public static IDictionary<string, TimeSpan> TimeByLocation(this IEnumerable<Entry> entries)
	{
		var dict = new Dictionary<string, TimeSpan>();

		foreach (var entry in entries)
		{
			var location = entry.Location?.Name ?? entry.Location?.Address ?? "unknown";
			var time = entry.Length();
			dict[location] = dict.GetValueOrDefault(location, TimeSpan.Zero) + time;
		}

		return dict;
	}

	public static IDictionary<string, TimeSpan> TimeByCategory(this IEnumerable<Entry> entries, IEnumerable<string> categories)
	{
		var dict = categories.ToDictionary(c => c, _ => TimeSpan.Zero);

		foreach (var entry in entries)
		{
			if (AddToCategory(dict, entry))
			{
				continue;
			}

			dict["other"] = dict.GetValueOrDefault("other", TimeSpan.Zero) + entry.Length();
		}

		return dict;

		static bool AddToCategory(IDictionary<string, TimeSpan> dict, Entry entry)
		{
			var time = entry.Length();
			foreach (var category in entry.Categories)
			{
				if (dict.TryGetValue(category, out var totalTime))
				{
					dict[category] = totalTime + time;
					return true;
				}
			}
			return false;
		}
	}

	public static IEnumerable<TimePeriod> ToTimePeriods(this IEnumerable<Entry> entries) =>
		entries.Select(e => new TimePeriod(e.Start.ToDateTime(), e.End?.ToDateTime() ?? DateTime.UtcNow));

	public static IDictionary<DayOfWeek, TimeSpan> TimeByDayOfWeek(this IEnumerable<TimePeriod> ranges)
	{
		var dict = new Dictionary<DayOfWeek, TimeSpan>
		{
			[DayOfWeek.Monday] = TimeSpan.Zero,
			[DayOfWeek.Tuesday] = TimeSpan.Zero,
			[DayOfWeek.Wednesday] = TimeSpan.Zero,
			[DayOfWeek.Thursday] = TimeSpan.Zero,
			[DayOfWeek.Friday] = TimeSpan.Zero,
			[DayOfWeek.Saturday] = TimeSpan.Zero,
			[DayOfWeek.Sunday] = TimeSpan.Zero,
		};

		foreach (var range in ranges)
		{
			foreach (var day in dict.Keys)
			{
				dict[day] += range.Intersection(day);
			}
		}

		return dict;
	}

	public static IDictionary<Week, TimeSpan> TimeByWeek(this IEnumerable<TimePeriod> ranges)
	{
		var dict = new Dictionary<Week, TimeSpan>();

		foreach (var range in ranges)
		{
			var startWeek = Week.FromDate(range.Start);
			dict[startWeek] = dict.GetValueOrDefault(startWeek, TimeSpan.Zero) + range.Intersection(startWeek);

			var endWeek = Week.FromDate(range.End);
			if (endWeek != startWeek)
			{
				dict[endWeek] = dict.GetValueOrDefault(endWeek, TimeSpan.Zero) + range.Intersection(endWeek);
			}
		}

		return dict;
	}

	public static IDictionary<HourOfDay, TimeSpan> TimeByHourOfDay(this IEnumerable<TimePeriod> ranges)
	{
		var dict = HourOfDay.All.ToDictionary(h => h, _ => TimeSpan.Zero);

		foreach (var range in ranges)
		{
			foreach (var hour in dict.Keys)
			{
				dict[hour] += range.Intersection(hour);
			}
		}

		return dict;
	}
}
