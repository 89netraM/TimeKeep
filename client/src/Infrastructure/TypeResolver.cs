using System;
using Spectre.Console.Cli;

namespace TimeKeep.Client.Infrastructure;

public sealed class TypeResolver : ITypeResolver, IDisposable
{
	private readonly IServiceProvider provider;

	public TypeResolver(IServiceProvider provider) =>
		this.provider = provider;

	public object? Resolve(Type? type) =>
		type is not null
			? provider.GetService(type)
			: null;

	public void Dispose()
	{
		if (provider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}