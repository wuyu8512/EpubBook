using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Wuyu.Epub.NcxItem;

namespace Wuyu.Epub
{
    public class Ncx
    {
        public XElement _baseElement;

        public Header Header { get; private set; }
        public DocTitle DocTitle { get; private set; }
        public NavMap NavMap { get; private set; }


        public Ncx(string content)
        {
            var _document = XDocument.Parse(content);
            _baseElement = _document.Root;

            // header
            {
                var header = _baseElement.Element(EpubBook.NcxNs + "head");
                if (header != null) Header = new Header(header);
                else
                {
                    Header = new Header();
                    _baseElement.Add(Header._baseElement);
                }
            }

            // docTitle
            {
                var docTitle = _baseElement.Element(EpubBook.NcxNs + "docTitle");
                if (docTitle != null) DocTitle = new DocTitle(docTitle);
                else
                {
                    DocTitle = new DocTitle();
                    _baseElement.Add(DocTitle._baseElement);
                }
            }

            // navMap
            {
                var map = _baseElement.Element(EpubBook.NcxNs + "navMap");
                if (map != null) NavMap = new NavMap(map);
                else
                {
                    NavMap = new NavMap();
                    _baseElement.Add(NavMap._baseElement);
                }
            }
        }
    }
}
