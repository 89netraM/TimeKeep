using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeKeep.Graphs.Svg;

public static class SvgGraph
{
	private const double Height = 130.0;
	private const double Width = 200.0;

	public static void WriteGraph<T>(IEnumerable<KeyValuePair<T, TimeSpan>> data, bool loop, string path)
		where T : notnull
	{
		var normalizedData = NormalizeData(data).ToArray();
		var unitWidth = CalculateUnitWidth(normalizedData.Length);

		var svg = new SvgDocument(-15, -5, Width + 30, Height + 20);
		AddLine(svg, normalizedData, loop, unitWidth);
		AddDots(svg, normalizedData, unitWidth);
		AddLabels(svg, normalizedData, unitWidth);
		AddYAxis(svg, data);
		svg.Save(path);
	}

	private static IEnumerable<(string, double)> NormalizeData<T>(IEnumerable<KeyValuePair<T, TimeSpan>> data)
		where T : notnull
	{
		var max = data.Max(t => t.Value);
		return data.Select(t => (t.Key.ToString() ?? "??", Height - Height * t.Value.TotalSeconds / max.TotalSeconds));
	}

	private static double CalculateUnitWidth(int dataLength) => Width / (dataLength - 1);

	private static void AddLine(SvgDocument svg, (string, double height)[] data, bool loop, double unitWidth)
	{
		DefineArea(svg);

		var startPath = BuildPath(data.Select(_ => Height).ToArray(), loop, unitWidth);
		var endPath = BuildPath(data.Select(d => d.height).ToArray(), loop, unitWidth);
		var pathElement = svg.Root.AppendElement("path");
		pathElement.SetAttribute("d", startPath);
		pathElement.SetAttribute("stroke", "#1690e0");
		pathElement.SetAttribute("fill", "none");
		pathElement.SetAttribute("clip-path", "url(#area)");
		pathElement.Animate("d", startPath, endPath);
	}

	private static void DefineArea(SvgDocument svg)
	{
		var area = svg.Defs.AppendElement("clipPath");
		area.SetAttribute("id", "area");
		var rect = area.AppendElement("rect");
		rect.SetAttribute("x", "-5");
		rect.SetAttribute("y", "-5");
		rect.SetAttribute("width", $"{Width + 20}");
		rect.SetAttribute("height", "100%");
	}

	private static string BuildPath(double[] data, bool loop, double unitWidth)
	{
		var path = new StringBuilder();
		int i = 0;
		if (loop)
		{
			path.Append($"M -10,{data[^1]:0.0000} S ");
		}
		else
		{
			path.Append($"M 0,{data[0]:0.0000} S ");
			i = 1;
		}
		for (; i < data.Length; i++)
		{
			var x = i * unitWidth;
			path.Append($"{x - unitWidth / 2.0},{data[i]:0.0000} {x},{data[i]:0.0000} ");
		}
		if (loop)
		{
			path.Append($"{Width + 15},{data[0]:0.0000} {(Width + 15) + unitWidth / 2.0},{data[0]:0.0000} ");
		}
		return path.ToString();
	}

	private static void AddDots(SvgDocument svg, (string, double height)[] data, double unitWidth)
	{
		DefineDot(svg);

		for (int i = 0; i < data.Length; i++)
		{
			AddDot(svg, i, data[i].height, unitWidth);
		}
	}

	private static void DefineDot(SvgDocument svg)
	{
		var dot = svg.Defs.AppendElement("circle");
		dot.SetAttribute("id", "dot");
		dot.SetAttribute("r", "1");
		dot.SetAttribute("fill", "#1690e0");
		dot.SetAttribute("stroke", "#000000");
		dot.SetAttribute("stroke-width", "0.5");
	}

	private static void AddDot(SvgDocument svg, int index, double height, double unitWidth)
	{
		var dot = svg.Root.AppendElement("use");
		dot.SetAttribute("href", "#dot");
		dot.SetAttribute("href", "http://www.w3.org/1999/xlink", "#dot");
		dot.SetAttribute("x", $"{index * unitWidth}");
		dot.SetAttribute("y", $"{Height:0.0000}");
		dot.Animate("y", $"{Height:0.0000}", $"{height:0.0000}");
	}

	private static void AddLabels(SvgDocument svg, (string label, double)[] data, double unitWidth)
	{
		for (int i = 0; i < data.Length; i++)
		{
			AddLabel(svg, i, data[i].label, unitWidth);
		}
	}

	private static void AddLabel(SvgDocument svg, int index, string label, double unitWidth)
	{
		var text = svg.Root.AppendElement("text");
		text.SetAttribute("x", $"{index * unitWidth}");
		text.SetAttribute("y", $"{Height + 10:0.0000}");
		text.SetAttribute("font-size", "5");
		text.SetAttribute("font-family", "sans-serif");
		text.SetAttribute("text-anchor", "middle");
		text.InnerText = label;
	}

	private static void AddYAxis<T>(SvgDocument svg, IEnumerable<KeyValuePair<T, TimeSpan>> data)
	{
		var max = data.Max(t => t.Value);
		var step = max / 10;
		for (int i = 0; i <= 10; i++)
		{
			AddYAxisLabel(svg, i, step * i);
		}
	}

	private static void AddYAxisLabel(SvgDocument svg, int index, TimeSpan time)
	{
		var text = svg.Root.AppendElement("text");
		text.SetAttribute("x", "-5");
		text.SetAttribute("y", $"{Height - Height * index / 10:0.0000}");
		text.SetAttribute("font-size", "4");
		text.SetAttribute("font-family", "sans-serif");
		text.SetAttribute("text-anchor", "end");
		var tspan = text.AppendElement("tspan");
		tspan.SetAttribute("alignment-baseline", "middle");
		tspan.InnerText = time.TotalHours.ToString("0\\h");
	}
}
