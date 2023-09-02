using System;

namespace TimeKeep.Common;

public class Interval
{
	public DateTime Start { get; }
	public DateTime End { get; }

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
}
