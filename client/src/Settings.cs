using System;

namespace TimeKeep.Client;

public class Settings
{
	public const string Section = "TimeKeep";

	public required Uri Url { get; init; }
	public required string Token { get; init; }
}
