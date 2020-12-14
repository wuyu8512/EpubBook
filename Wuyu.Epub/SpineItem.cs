using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class SpineItem : EpubXElementItem
    {
        internal static XName Name { get; } = EpubBook.OpfNs + "itemref";

        public string IdRef
        {
            get => BaseElement.Attribute("idref")?.Value;
            set => BaseElement.SetAttributeValue("idref", value);
        }

        public SpineItem(XElement baseElement)
        {
            if (baseElement.Name != Name) throw new ArgumentException("baseElement的名称应当为" + Name);
            BaseElement = baseElement;
        }

        public SpineItem()
        {
            BaseElement = new XElement(Name);
        }

        public SpineItem(string idRef)
        {
            BaseElement = new XElement(Name);
            this.IdRef = idRef;
        }
    }
}