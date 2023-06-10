using System;
using System.Globalization;

namespace TimeKeep.Graphs;

public readonly record struct Week(int WeekNumber) : IComparable<Week>
{
	public static Week FromDate(DateTime date)
	{
		var cal = CultureInfo.CurrentCulture.Calendar;
		return new(cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
	}

	public int CompareTo(Week other) => WeekNumber.CompareTo(other.WeekNumber);

	public override string ToString() => WeekNumber.ToString();
}
