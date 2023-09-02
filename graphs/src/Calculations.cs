using System;
using System.Collections.Generic;
using System.Linq;
using TimeKeep.Common;
using TimeKeep.RPC.Entries;

namespace TimeKeep.Graphs;

public static class Calculations
{
	public static IDictionary<string, Duration> TimeByLocation(this IEnumerable<Entry> entries)
	{
		var dict = new Dictionary<string, Duration>();

		foreach (var entry in entries)
		{
			var location = entry.Location?.Name ?? entry.Location?.Address ?? "unknown";
			var interval = entry.ToInterval();
			if (!dict.TryGetValue(location, out var duration))
			{
				duration = dict[location] = new Duration();
			}
			duration.AddInterval(interval);
		}

		return dict;
	}

	public static IDictionary<string, Duration> TimeByCategory(this IEnumerable<Entry> entries, IEnumerable<string> categories)
	{
		var dict = categories.ToDictionary(c => c, _ => new Duration());

		foreach (var entry in entries)
		{
			if (AddToCategory(dict, entry))
			{
				continue;
			}

			var interval = entry.ToInterval();
			if (!dict.TryGetValue("other", out var duration))
			{
				duration = dict["other"] = new Duration();
			}
			duration.AddInterval(interval);
		}

		return dict;

		static bool AddToCategory(IDictionary<string, Duration> dict, Entry entry)
		{
			var interval = entry.ToInterval();
			foreach (var category in entry.Categories)
			{
				if (dict.TryGetValue(category, out var duration))
				{
					duration.AddInterval(interval);
					return true;
				}
			}
			return false;
		}
	}

	public static Duration ToDuration(this IEnumerable<Entry> entries)
	{
		var duration = new Duration();
		foreach (var entry in entries)
		{
			duration.AddInterval(entry.ToInterval());
		}
		return duration;
	}

	public static IDictionary<DayOfWeek, Duration> TimeByDayOfWeek(this Duration duration) =>
		Enum.GetValues<DayOfWeek>()
			.ToDictionary(
				day => day,
				duration.Intersect);

	public static IDictionary<Week, Duration> TimeByWeek(this Duration duration) =>
		Week.All
			.ToDictionary(
				day => day,
				duration.Intersect);

	public static IDictionary<HourOfDay, Duration> TimeByHourOfDay(this Duration duration) =>
		HourOfDay.All
			.ToDictionary(
				day => day,
			duration.Intersect);
}
