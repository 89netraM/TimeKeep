using System;

namespace TimeKeep.Common;

public static class DateTimeExtensions
{
	public static DateTime NextHour(this DateTime dateTime)
	{
		var inOneHour = dateTime.AddHours(1);
		return new DateTime(inOneHour.Year, inOneHour.Month, inOneHour.Day, inOneHour.Hour, 0, 0);
	}

	public static DateTime NextWeek(this DateTime dateTime) =>
		dateTime.Date
			.AddDays(dateTime.DayOfWeek switch
			{
				DayOfWeek.Monday => 7,
				DayOfWeek.Tuesday => 6,
				DayOfWeek.Wednesday => 5,
				DayOfWeek.Thursday => 4,
				DayOfWeek.Friday => 3,
				DayOfWeek.Saturday => 2,
				DayOfWeek.Sunday => 1,
				_ => throw new InvalidOperationException(),
			});
}
