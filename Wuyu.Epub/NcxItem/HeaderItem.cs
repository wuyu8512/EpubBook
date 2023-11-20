using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class HeaderItem : EpubXElementItem
    {
        protected override XName ItemName => EpubBook.NcxNs + "meta";

        public string Name
        {
            get => _baseElement.Attribute("name")?.Value;
            set => _baseElement.SetAttributeValue("name", value);
        }

        public string Content
        {
            get => _baseElement.Attribute("content")?.Value;
            set => _baseElement.SetAttributeValue("content", value);
        }

        public HeaderItem(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException($"{nameof(element)}的名称应当为" + ItemName);
            _baseElement = element;
        }

        public HeaderItem()
        {
            _baseElement = new XElement(ItemName);
        }
    }
}
