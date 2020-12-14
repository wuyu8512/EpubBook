using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class GuideItem : EpubXElementItem
    {
        internal static XName Name { get; } = EpubBook.OpfNs + "reference";

        public string Type
        {
            get => BaseElement.Attribute("type")?.Value;
            set => BaseElement.SetAttributeValue("type", value);
        }

        public string Title
        {
            get => BaseElement.Attribute("title")?.Value;
            set => BaseElement.SetAttributeValue("title", value);
        }

        public string Href
        {
            get => BaseElement.Attribute("href")?.Value;
            set => BaseElement.SetAttributeValue("href", value);
        }

        public GuideItem(XElement baseElement)
        {
            if (baseElement.Name != Name) throw new ArgumentException("baseElement的名称应当为" + Name);
            BaseElement = baseElement;
        }

        public GuideItem()
        {
            BaseElement = new XElement(Name);
        }
    }
}