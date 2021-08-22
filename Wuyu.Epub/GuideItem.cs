using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class GuideItem : EpubXElementItem
    {
        protected override XName ItemName => EpubBook.OpfNs + "reference";

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
            if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            BaseElement = baseElement;
        }

        public GuideItem()
        {
            BaseElement = new XElement(ItemName);
        }
    }
}