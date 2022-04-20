using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class Header : EpubXElement<HeaderItem>
    {
        protected override XName ItemName => EpubBook.OpfNs + "meta";

        public Header(XElement xElement): base(xElement)
        {

        }

        public Header() : base(EpubBook.OpfNs + "head")
        {

        }
    }
}
