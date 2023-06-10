using System;
using System.Xml;

namespace TimeKeep.Graphs.Svg;

public static class SvgElement
{
	public static XmlElement AppendElement(this XmlElement element, string name)
	{
		var child = element.OwnerDocument.CreateElement(name, "http://www.w3.org/2000/svg");
		element.AppendChild(child);
		return child;
	}

	public static void Animate(this XmlElement element, string attributeName, params string[] values)
	{
		var animate = element.AppendCommonAnimation("animate");
		animate.SetAttribute("attributeName", attributeName);
		animate.SetAttribute("values", String.Join("; ", values));
	}

	public static void AnimateTransform(this XmlElement element, string type, params string[] values)
	{
		var animateTransform = element.AppendCommonAnimation("animateTransform");
		animateTransform.SetAttribute("attributeName", "transform");
		animateTransform.SetAttribute("type", type);
		animateTransform.SetAttribute("values", String.Join("; ", values));
	}

	public static void AnimateMotion(this XmlElement element, string path)
	{
		var animateMotion = element.AppendCommonAnimation("animateMotion");
		animateMotion.SetAttribute("path", path);
	}

	private static XmlElement AppendCommonAnimation(this XmlElement element, string animationType)
	{
		var animate = element.AppendElement(animationType);
		animate.SetAttribute("repeatCount", "1");
		animate.SetAttribute("dur", "1s");
		animate.SetAttribute("begin", "0s");
		animate.SetAttribute("fill", "freeze");
		animate.SetAttribute("calcMode", "spline");
		animate.SetAttribute("keyTimes", "0; 1");
		animate.SetAttribute("keySplines", "0,0,0.2,1");
		return animate;
	}
}
