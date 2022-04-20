using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Wuyu.Epub.NcxItem;

namespace Wuyu.Epub
{
    public class Ncx
    {
        public XElement BaseElement { get; private set; }

        public Header Header { get; private set; }
        public DocTitle DocTitle { get; private set; }
        public NavMap NavMap { get; private set; }


        public Ncx(string content)
        {
            var _document = XDocument.Parse(content);
            BaseElement = _document.Root;

            // header
            {
                var header = BaseElement.Element(EpubBook.NcxNs + "head");
                if (header != null) Header = new Header(header);
                else
                {
                    Header = new Header();
                    BaseElement.Add(Header.BaseElement);
                }
            }

            // docTitle
            {
                var docTitle = BaseElement.Element(EpubBook.NcxNs + "docTitle");
                if (docTitle != null) DocTitle = new DocTitle(docTitle);
                else
                {
                    DocTitle = new DocTitle();
                    BaseElement.Add(DocTitle.BaseElement);
                }
            }

            // navMap
            {
                var map = BaseElement.Element(EpubBook.NcxNs + "navMap");
                if (map != null) NavMap = new NavMap(map);
                else
                {
                    NavMap = new NavMap();
                    BaseElement.Add(NavMap.BaseElement);
                }
            }
        }
    }
}
