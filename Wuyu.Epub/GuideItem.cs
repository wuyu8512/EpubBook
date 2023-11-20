using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class GuideItem : EpubXElementItem
    {
        protected override XName ItemName => EpubBook.OpfNs + "reference";

        public string Type
        {
            get => _baseElement.Attribute("type")?.Value;
            set => _baseElement.SetAttributeValue("type", value);
        }

        public string Title
        {
            get => _baseElement.Attribute("title")?.Value;
            set => _baseElement.SetAttributeValue("title", value);
        }

        public string Href
        {
            get => _baseElement.Attribute("href")?.Value;
            set => _baseElement.SetAttributeValue("href", value);
        }

        public GuideItem(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException("Element的名称应当为" + ItemName);
            _baseElement = element;
        }

        public GuideItem()
        {
            _baseElement = new XElement(ItemName);
        }
    }
}