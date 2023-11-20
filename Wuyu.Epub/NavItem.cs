using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class NavItem : EpubXElementItem
    {
        protected override XName ItemName { get; } = EpubBook.XHtmlNs + "li";

        public string Href
        {
            get => _baseElement.Element(EpubBook.XHtmlNs + "a").Attribute("href").Value;
            set
            {
                var a = _baseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    _baseElement.Add(a);
                }
                a.SetAttributeValue("href", value);
            }
        }

        public string Title
        {
            get => _baseElement.Element(EpubBook.XHtmlNs + "a").Value;
            set
            {
                var a = _baseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    _baseElement.Add(a);
                }
                a.Value = value;
            }
        }

        public string Type
        {
            get => _baseElement.Element(EpubBook.XHtmlNs + "a").Attribute(EpubBook.EpubNs + "type").Value;
            set
            {
                var a = _baseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    _baseElement.Add(a);
                }
                a.SetAttributeValue(EpubBook.EpubNs + "type", value);
            }
        }

        public NavItem(XElement element)
        {
            _baseElement = element;
        }

        public NavItem()
        {
            _baseElement = new XElement(ItemName);
        }
    }
}
