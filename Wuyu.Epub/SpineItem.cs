using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class SpineItem : EpubXElementItem
    {
        protected override XName ItemName { get; } = EpubBook.OpfNs + "itemref";

        public string IdRef
        {
            get => BaseElement.Attribute("idref")?.Value;
            set => BaseElement.SetAttributeValue("idref", value);
        }

        public SpineItem(XElement baseElement)
        {
            if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            BaseElement = baseElement;
        }

        public SpineItem()
        {
            BaseElement = new XElement(ItemName);
        }

        public SpineItem(string idRef)
        {
            BaseElement = new XElement(ItemName);
            this.IdRef = idRef;
        }
    }
}