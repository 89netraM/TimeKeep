using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TimeKeep.Stats.Services;

namespace TimeKeep.Stats.ViewComponents;

public class WeekViewViewComponent(WeekService weekService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(
        string[] categories,
        CancellationToken cancellationToken
    )
    {
        var weeks = await weekService.FetchWeeks(categories, cancellationToken);
        var hue = GenerateHue(categories);
        return View((hue, weeks));
    }

    private static int GenerateHue(string[] categories)
    {
        var hash = categories.Sum(SumChars);
        var rng = new Random(hash);
        return rng.Next(0, 360);

        static int SumChars(string s) => s.Sum(CharToInt);
        static int CharToInt(char c) => c;
    }
}
