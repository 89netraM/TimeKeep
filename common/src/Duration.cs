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

	public Duration Intersect(DayOfWeek day) =>
		Intersect(interval => interval.Intersect(day));

	public Duration Intersect(Week week) =>
		Intersect(interval => interval.Intersect(week));

	public Duration Intersect(HourOfDay hour) =>
		Intersect(interval => interval.Intersect(hour));

	public Duration Intersect(Func<Interval, IEnumerable<Interval>> intersector)
	{
		var duration = new Duration();
		foreach (var interval in intervals)
		{
			foreach (var intersectingInterval in intersector(interval))
			{
				duration.AddInterval(intersectingInterval);
			}
		}
		return duration;
	}
}
