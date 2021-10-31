using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Wuyu.Epub.Properties;
using System.Linq;
using System.IO;

namespace Wuyu.Epub
{
    public class Nav : EpubXElement<NavItem>, IList<NavItem>
    {
        protected override XName ItemName { get; } = EpubBook.XHtmlNs + "li";

        public NavGuide NavGuide;

        public Nav() : base(
            XDocument.Parse(Resources.nav)
            .Descendants(EpubBook.XHtmlNs + "nav")
            .SingleOrDefault(a => a.Attribute(EpubBook.EpubNs + "type")?.Value == "toc")
            .Element(EpubBook.XHtmlNs + "ol")
            )
        {
            this.NavGuide = new(this.BaseElement.Document);
        }

        public Nav(string content) : base(
            XDocument.Parse(content)
            .Descendants(EpubBook.XHtmlNs + "nav")
            .SingleOrDefault(a => a.Attribute(EpubBook.EpubNs + "type")?.Value == "toc")
            .Element(EpubBook.XHtmlNs + "ol")
            )
        {
            var guide = this.BaseElement.Document
                .Descendants(EpubBook.XHtmlNs + "nav")
                .SingleOrDefault(a => a.Attribute(EpubBook.EpubNs + "type")?.Value == "landmarks");
            if (guide == default)
            {
                var contentElement = XElement.Parse(Resources.navGuideCover);
                contentElement.FirstAttribute.Remove();
                contentElement.FirstAttribute.Remove();
                var body = this.BaseElement.Document.Descendants(EpubBook.XHtmlNs + "body").First();
                body.Add(contentElement);
            }

            this.NavGuide = new(this.BaseElement.Document);
        }
    }
}
