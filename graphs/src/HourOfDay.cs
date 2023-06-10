using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeKeep.Graphs;

public readonly record struct HourOfDay(int Hour) : IComparable<HourOfDay>
{
	public static IEnumerable<HourOfDay> All { get; } = Enumerable.Range(0, 24).Select(h => new HourOfDay(h));

	public int CompareTo(HourOfDay other) => Hour.CompareTo(other.Hour);

	public override string ToString() => Hour.ToString("00");
}
