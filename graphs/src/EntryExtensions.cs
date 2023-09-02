using System;
using TimeKeep.Common;
using TimeKeep.RPC.Entries;

namespace TimeKeep.Graphs;

public static class EntryExtensions
{
	public static Interval ToInterval(this Entry entry) =>
		new Interval(entry.Start.ToDateTime(), entry.End?.ToDateTime() ?? DateTime.UtcNow);
}
