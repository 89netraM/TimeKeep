using System;

namespace TimeKeep.Graphs;

public class Settings
{
	public const string Section = "TimeKeep";

	public required Uri Url { get; init; }
	public required string Token { get; init; }
}
