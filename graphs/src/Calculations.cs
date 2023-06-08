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
}
