using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class SpineItem : EpubXElementItem
    {
        protected override XName ItemName { get; } = EpubBook.OpfNs + "itemref";

        public string IdRef
        {
            get => _baseElement.Attribute("idref")?.Value;
            set => _baseElement.SetAttributeValue("idref", value);
        }

        public SpineItem(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException($"{nameof(element)}的名称应当为" + ItemName);
            _baseElement = element;
        }

        public SpineItem()
        {
            _baseElement = new XElement(ItemName);
        }

        public SpineItem(string idRef)
        {
            _baseElement = new XElement(ItemName);
            this.IdRef = idRef;
        }
    }
}