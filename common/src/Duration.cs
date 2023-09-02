using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeKeep.Common;

public class Duration
{
	private readonly IList<Interval> intervals = new List<Interval>();

	public TimeSpan TotalDuration => intervals
		.Select(interval => interval.End - interval.Start)
		.Aggregate(TimeSpan.Zero, (total, duration) => total + duration);

	public void AddInterval(DateTime start, DateTime end) =>
		AddInterval(new Interval(start, end));

	public void AddInterval(Interval newInterval)
	{
		foreach (var interval in intervals)
		{
			if (interval.Merge(newInterval) is Interval mergedInterval)
			{
				intervals.Remove(interval);
				AddInterval(mergedInterval);
				return;
			}
		}

		intervals.Add(newInterval);
	}
}
