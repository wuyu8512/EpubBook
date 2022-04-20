using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class NavMap : EpubXElement<NavPoint>
    {
        protected override XName ItemName => EpubBook.NcxNs + "navPoint";

        public NavMap(XElement xElement) : base(xElement)
        {

        }

        public NavMap() : base(EpubBook.OpfNs + "navMap")
        {

        }
    }
}
