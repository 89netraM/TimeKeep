using System;
using System.Xml;

namespace TimeKeep.Graphs.Svg;

public class SvgDocument
{
	private readonly XmlDocument document;
	public XmlElement Root { get; }

	private Lazy<XmlElement> defs;
	public XmlElement Defs => defs.Value;

	public SvgDocument(double x, double y, double width, double height)
	{
		document = new XmlDocument();
		document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", null));
		Root = CreateRoot(document, x, y, width, height);
		document.AppendChild(Root);

		defs = new(CreateDefs);

		static XmlElement CreateRoot(XmlDocument document, double x, double y, double width, double height)
		{
			var root = document.CreateElement("svg", "http://www.w3.org/2000/svg");
			root.SetAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");
			root.SetAttribute("version", "1.1");
			root.SetAttribute("viewBox", $"{x} {y} {width} {height}");
			return root;
		}

		XmlElement CreateDefs() =>
			Root.AppendElement("defs");
	}

	public void Save(string path)
	{
		document.Save(path);
	}
}
