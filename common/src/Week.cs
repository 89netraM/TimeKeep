using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TimeKeep.Common;

public readonly record struct Week(int WeekNumber) : IComparable<Week>
{
	public static IEnumerable<Week> All { get; } = Enumerable.Range(0, 53).Select(w => new Week(w));

	public static Week FromDate(DateTime date)
	{
		var cal = CultureInfo.CurrentCulture.Calendar;
		return new(cal.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
	}

	public int CompareTo(Week other) => WeekNumber.CompareTo(other.WeekNumber);

	public override string ToString() => WeekNumber.ToString();
}
