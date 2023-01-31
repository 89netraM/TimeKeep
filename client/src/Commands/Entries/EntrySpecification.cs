using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using Grpc.Core;
using TimeKeep.RPC.Entries;
using static TimeKeep.RPC.Entries.EntriesService;

namespace TimeKeep.Client.Commands.Entries;

public class EntrySpecificationService
{
	private readonly EntriesServiceClient client;

	public EntrySpecificationService(EntriesServiceClient client) =>
		this.client = client;

	public ValueTask<Guid?> GetGuidOf(EntrySpecification spec) => spec switch
	{
		EntryGuid(Guid id) => new ValueTask<Guid?>(id),
		EntryIndex(int index) => new ValueTask<Guid?>(FetchGuidOf(index)),
		_ => throw new ArgumentException($"Unknown entry id ({spec})", nameof(spec)),
	};

	private async Task<Guid?> FetchGuidOf(int index)
	{
		int i = 0;
		await foreach (var entry in client.List(new() { EndStatus = EndStatus.OnlyActive, Order = Order.StartDesc }).ResponseStream.ReadAllAsync())
		{
			if (i == index)
			{
				return Guid.Parse(entry.Id);
			}
			i++;
		}
		return null;
	}
}

[TypeConverter(typeof(EntrySpecificationConverter))]
public abstract record EntrySpecification();
public record EntryGuid(Guid Id) : EntrySpecification();
public record EntryIndex(int Index) : EntrySpecification();

public class EntrySpecificationConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType) =>
		destinationType == typeof(EntrySpecification);

	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is string input)
		{
			if (Guid.TryParse(input, out Guid guid))
			{
				return new EntryGuid(guid);
			}
			else if (input[0] == '^' && int.TryParse(input[1..], out int index) && index >= 0)
			{
				return new EntryIndex(index);
			}
		}

		return null;
	}

	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return value switch
			{
				EntryGuid(Guid guid) => guid.ToString(),
				EntryIndex(int index) => $"^{index}",
				_ => null,
			};
		}

		return null;
	}
}
