using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TimeKeep.Common;
using TimeKeep.RPC.Entries;

namespace TimeKeep.Stats.Services;

public class WeekService(
    ILogger<WeekService> logger,
    EntriesService.EntriesServiceClient entriesClient
)
{
    public async Task<Dictionary<DateOnly, Week>> FetchWeeks(
        IEnumerable<string> categories,
        CancellationToken cancellationToken
    )
    {
        var days = await FetchDailyTimeSpent(categories, cancellationToken);
        if (days.Count is 0)
        {
            return [];
        }

        var firstDate = days.Keys.Min();
        var firstMonday = firstDate.AddDays(-(((int)firstDate.DayOfWeek + 6) % 7));
        var lastDate = days.Keys.Max();

        var weeks = new Dictionary<DateOnly, Week>();
        for (var date = firstMonday; date <= lastDate; date = date.AddDays(7))
        {
            weeks.Add(
                date,
                new(
                    days.GetTimeSpent(date),
                    days.GetTimeSpent(date.AddDays(1)),
                    days.GetTimeSpent(date.AddDays(2)),
                    days.GetTimeSpent(date.AddDays(3)),
                    days.GetTimeSpent(date.AddDays(4)),
                    days.GetTimeSpent(date.AddDays(5)),
                    days.GetTimeSpent(date.AddDays(6))
                )
            );
        }

        return weeks;
    }

    private async Task<Dictionary<DateOnly, Duration>> FetchDailyTimeSpent(
        IEnumerable<string> categories,
        CancellationToken cancellationToken
    )
    {
        var days = new Dictionary<DateOnly, Duration>();

        logger.LogInformation("Fetching daily time spent on {@Categories}", categories);
        var response = entriesClient.List(
            new() { Categories = { categories } },
            cancellationToken: cancellationToken
        );
        await foreach (
            var entry in response.ResponseStream.ReadAllAsync(cancellationToken: cancellationToken)
        )
        {
            days.Add(entry);
        }

        return days;
    }
}

public record Week(
    TimeSpan Monday,
    TimeSpan Tuesday,
    TimeSpan Wednesday,
    TimeSpan Thursday,
    TimeSpan Friday,
    TimeSpan Saturday,
    TimeSpan Sunday
)
{
    public TimeSpan Max =>
        Monday.Max(Tuesday).Max(Wednesday).Max(Thursday).Max(Friday).Max(Saturday).Max(Sunday);
}

file static class DictionaryDateOnlyDayExtensions
{
    public static void Add(this Dictionary<DateOnly, Duration> days, Entry entry)
    {
        var start = entry.Start.ToDateTime();
        var end = entry.End?.ToDateTime() ?? DateTime.Now;

        for (var date = start.Date; date < end; date = date.AddDays(1))
        {
            var dateOnly = DateOnly.FromDateTime(date);
            if (!days.TryGetValue(dateOnly, out var duration))
            {
                duration = new();
                days.Add(dateOnly, duration);
            }

            duration.AddInterval(start.Max(date), end.Min(date.AddDays(1)));
        }
    }

    public static TimeSpan GetTimeSpent(this Dictionary<DateOnly, Duration> days, DateOnly date) =>
        days.TryGetValue(date, out var monday) ? monday.TotalDuration : TimeSpan.Zero;
}

file static class DateTimeExtensions
{
    public static DateTime Max(this DateTime a, DateTime b) => a > b ? a : b;

    public static DateTime Min(this DateTime a, DateTime b) => a < b ? a : b;
}

file static class TimeSpanExtensions
{
    public static TimeSpan Max(this TimeSpan a, TimeSpan b) => a > b ? a : b;
}
