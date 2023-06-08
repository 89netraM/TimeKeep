using System;

namespace TimeKeep.Graphs;

public readonly record struct TimePeriod(DateTime Start, DateTime End)
{
	public TimeSpan Intersection(DayOfWeek day)
	{
		if (Start.DayOfWeek == day && End.DayOfWeek == day)
		{
			return End - Start;
		}

		if (Start.DayOfWeek == day)
		{
			return Start.Date.AddDays(1) - Start;
		}

		if (End.DayOfWeek == day)
		{
			return End - End.Date;
		}

		return TimeSpan.Zero;
	}

	public TimeSpan Intersection(Week week)
	{
		var startWeek = Week.FromDate(Start);
		var endWeek = Week.FromDate(End);

		if (startWeek == week && endWeek == week)
		{
			return End - Start;
		}

		if (startWeek == week)
		{
			return Start.Date.AddDays(7) - Start;
		}

		if (endWeek == week)
		{
			return End - End.Date;
		}

		return TimeSpan.Zero;
	}

	public TimeSpan Intersection(HourOfDay hour)
	{
		if (Start.Hour == hour.Hour && End.Hour == hour.Hour)
		{
			return End - Start;
		}

		if (Start.Hour == hour.Hour)
		{
			return Start.Date.AddHours(hour.Hour + 1) - Start;
		}

		if (End.Hour == hour.Hour)
		{
			return End - End.Date.AddHours(hour.Hour);
		}

		return TimeSpan.Zero;
	}
}
