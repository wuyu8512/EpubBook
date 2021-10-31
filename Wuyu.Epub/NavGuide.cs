using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class NavGuide : EpubXElement<NavItem>, IList<NavItem>
    {
        protected override XName ItemName { get; } = EpubBook.XHtmlNs + "li";

        public NavGuide(XDocument document) : base(
            document
            .Descendants(EpubBook.XHtmlNs + "nav")
            .SingleOrDefault(a => a.Attribute(EpubBook.EpubNs + "type")?.Value == "landmarks")
            .Element(EpubBook.XHtmlNs + "ol")
            )
        {
        }
    }
}
