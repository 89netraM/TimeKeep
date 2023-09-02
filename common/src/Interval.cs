using System;
using System.Collections.Generic;

namespace TimeKeep.Common;

public class Interval
{
	public DateTime Start { get; }
	public DateTime End { get; }

	public TimeSpan Duration => End - Start;

	public Interval(DateTime start, DateTime end)
	{
		if (end < start)
		{
			throw new ArgumentException("An interval cannot end before it has started", nameof(end));
		}

		Start = start;
		End = end;
	}
	
	public Interval? Merge(Interval other)
	{
		if (Contains(other.Start) & Contains(other.End))
		{
			return this;
		}
		if (Contains(other.Start))
		{
			return new Interval(Start, other.End);
		}
		if (Contains(other.End))
		{
			return new Interval(other.Start, End);
		}
		if (other.Contains(Start) && other.Contains(End))
		{
			return other;
		}
		return null;
	}

	public bool Contains(DateTime instant) =>
		Start <= instant && instant <= End;

	public IEnumerable<Interval> Intersect(DayOfWeek day) =>
		Intersect(
			i => i.Start.DayOfWeek == day && i.End.DayOfWeek == day && i.Duration < TimeSpan.FromDays(1),
			d => d.Date.AddDays(1),
			d => d.DayOfWeek == day);

	public IEnumerable<Interval> Intersect(HourOfDay hour) =>
		Intersect(
			i => i.Start.Hour == hour.Hour && i.End.Hour == hour.Hour && i.Duration < TimeSpan.FromHours(1),
			d => d.NextHour(),
			d => d.Hour == hour.Hour);

	public IEnumerable<Interval> Intersect(Week week) =>
		Intersect(
			i => Week.FromDate(i.Start) == week && Week.FromDate(i.End) == week && i.Duration < TimeSpan.FromDays(7),
			d => d.NextWeek(),
			d => Week.FromDate(d) == week);

	public IEnumerable<Interval> Intersect(
		Func<Interval, bool> isContained,
		Func<DateTime, DateTime> moveToBeginningOfNext,
		Func<DateTime, bool> isStartIn)
	{
		if (isContained(this))
		{
			yield return this;
			yield break;
		}

		var split = moveToBeginningOfNext(Start);
		if (split > End)
		{
			yield break;
		}
		
		if (isStartIn(Start))
		{
			yield return new Interval(Start, split);
		}

		if (split == End)
		{
			yield break;
		}

		var rest = new Interval(split, End)
			.Intersect(isContained, moveToBeginningOfNext, isStartIn);
		foreach (var intersection in rest)
		{
			yield return intersection;
		}
	}
}
