﻿using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace TimeKeep.App.Desktop;

internal class Program
{
	[STAThread]
	public static void Main(string[] args) =>
		BuildAvaloniaApp()
			.StartWithClassicDesktopLifetime(args);

	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();
}
