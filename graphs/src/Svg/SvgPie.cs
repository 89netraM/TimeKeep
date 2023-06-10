using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace TimeKeep.Graphs.Svg;

public static class SvgPie
{
	public static SvgDocument WritePie(IEnumerable<KeyValuePair<string, TimeSpan>> data)
	{
		var normalizedData = NormalizeData(data);

		var svg = new SvgDocument(-2, -1.5, 4, 3);
		AddPieSlices(svg, normalizedData);
		AddLabels(svg, data, normalizedData);
		return svg;
	}

	private static (string, double)[] NormalizeData(IEnumerable<KeyValuePair<string, TimeSpan>> data)
	{
		var total = data.Aggregate(TimeSpan.Zero, (t, s) => t + s.Value);
		return data.Select(d => (d.Key, d.Value.TotalSeconds / total.TotalSeconds)).ToArray();
	}

	private static void AddPieSlices(SvgDocument svg, (string, double)[] data)
	{
		DefinePieCutout(svg);

		var group = DefinePieGroup(svg);
		var percentageOffset = 0.0;
		foreach (var ((_, percentage), color) in data.Zip(PieSliceColors()))
		{
			AddPieSlice(group, percentageOffset, percentage, color);
			percentageOffset += percentage;
		}
	}

	private static void DefinePieCutout(SvgDocument svg)
	{
		var clipPath = svg.Defs.AppendElement("clipPath");
		clipPath.SetAttribute("id", "pie");
		var circle = clipPath.AppendElement("circle");
		circle.SetAttribute("r", "1");
	}

	private static XmlElement DefinePieGroup(SvgDocument svg)
	{
		var group = svg.Root.AppendElement("g");
		group.SetAttribute("transform", "rotate(-90)");
		group.SetAttribute("clip-path", "url(#pie)");
		return group;
	}

	private static IEnumerable<string> PieSliceColors()
	{
		while (true)
		{
			yield return "#7fba00";
			yield return "#ffb900";
			yield return "#00a4ef";
			yield return "#f74e1e";
			yield return "#8045ff";
			yield return "#0046ff";
			yield return "#ff5b10";
			yield return "#08b1e1";
		}
	}

	private static void AddPieSlice(XmlElement group, double offsetPercentage, double percentage, string color)
	{
		var slice = group.AppendElement("circle");
		slice.SetAttribute("fill", "none");
		slice.SetAttribute("stroke", color);
		slice.SetAttribute("r", "1");
		slice.SetAttribute("stroke-width", "2");
		slice.SetAttribute("stroke-dasharray", $"0 {Math.PI * 2:0.0000}");
		slice.Animate("stroke-dasharray", $"0 {Math.PI * 2:0.0000}", $"{Math.PI * 2 * percentage:0.0000} {Math.PI * 2:0.0000}");
		if (offsetPercentage > 0.0)
		{
			slice.AnimateTransform("rotate", "0", $"{offsetPercentage * 360:0.0000}");
		}
	}

	private static void AddLabels(SvgDocument svg, IEnumerable<KeyValuePair<string, TimeSpan>> data, (string, double)[] normalizedData)
	{
		var group = CreateLabelGroup(svg);
		var percentageOffset = 0.0;
		foreach (var ((label, percentage), (_, duration)) in normalizedData.Zip(data))
		{
			AddLabel(group, percentageOffset, percentage, label, duration);
			percentageOffset += percentage;
		}
	}

	private static XmlElement CreateLabelGroup(SvgDocument svg)
	{
		var group = svg.Root.AppendElement("g");
		group.SetAttribute("transform", "scale(1.25)");
		return group;
	}

	private static void AddLabel(XmlElement group, double offsetPercentage, double percentage, string label, TimeSpan duration)
	{
		offsetPercentage += percentage / 2;

		var text = group.AppendElement("text");
		text.SetAttribute("font-size", "0.1");
		text.SetAttribute("font-family", "sans-serif");
		text.SetAttribute("text-anchor", "middle");

		var largeArc = IsLargeArc(offsetPercentage) ? "1" : "0";
		var (x, y) = GetArcCoordinates(offsetPercentage);
		text.AnimateMotion($"M 0,-1 A 1,1 0 {largeArc} 1 {x:0.0000},{y:0.0000}");

		var labelSpan = text.AppendElement("tspan");
		labelSpan.SetAttribute("alignment-baseline", "middle");
		labelSpan.InnerText = $"{label} {duration.TotalHours:0}h";
	}

	private static bool IsLargeArc(double percentage) => percentage > 0.5;

	private static (double, double) GetArcCoordinates(double percentage)
	{
		var radians = Math.PI * 2 * percentage;
		var x = Math.Sin(radians);
		var y = -Math.Cos(radians);
		return (x, y);
	}
}
