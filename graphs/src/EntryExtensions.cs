using System;
using TimeKeep.RPC.Entries;

namespace TimeKeep.Graphs;

public static class EntryExtensions
{
	public static TimeSpan Length(this Entry entry) =>
		(entry.End?.ToDateTime() ?? DateTime.UtcNow) - entry.Start.ToDateTime();
}
