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
            get => BaseElement.Attribute("name")?.Value;
            set => BaseElement.SetAttributeValue("name", value);
        }

        public string Content
        {
            get => BaseElement.Attribute("content")?.Value;
            set => BaseElement.SetAttributeValue("content", value);
        }

        public HeaderItem(XElement baseElement)
        {
            if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            BaseElement = baseElement;
        }

        public HeaderItem()
        {
            BaseElement = new XElement(ItemName);
        }
    }
}
