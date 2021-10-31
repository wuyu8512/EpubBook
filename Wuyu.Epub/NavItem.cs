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
            get => BaseElement.Element(EpubBook.XHtmlNs + "a").Attribute("href").Value;
            set
            {
                var a = BaseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    BaseElement.Add(a);
                }
                a.SetAttributeValue("href", value);
            }
        }

        public string Title
        {
            get => BaseElement.Element(EpubBook.XHtmlNs + "a").Value;
            set
            {
                var a = BaseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    BaseElement.Add(a);
                }
                a.Value = value;
            }
        }

        public string Type
        {
            get => BaseElement.Element(EpubBook.XHtmlNs + "a").Attribute(EpubBook.EpubNs + "type").Value;
            set
            {
                var a = BaseElement.Element(EpubBook.XHtmlNs + "a");
                if (a == null)
                {
                    a = new XElement(EpubBook.XHtmlNs + "a");
                    BaseElement.Add(a);
                }
                a.SetAttributeValue(EpubBook.EpubNs + "type", value);
            }
        }

        public NavItem(XElement element)
        {
            BaseElement = element;
        }

        public NavItem()
        {
            BaseElement = new XElement(ItemName);
        }
    }
}
