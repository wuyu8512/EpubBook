using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class DocTitle : EpubXElementItem
    {
        protected override XName ItemName => EpubBook.NcxNs + "docTitle";

        public string Name
        {
            get => _baseElement.Element(EpubBook.NcxNs + "text")?.Value;
            set
            {
                var textElement = _baseElement.Element(EpubBook.NcxNs + "text");
                if (textElement == null)
                {
                    textElement = new XElement(EpubBook.NcxNs + "text");
                    _baseElement.Add(textElement);
                }
                textElement.Value = value;
            }
        }

        public DocTitle(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException($"{nameof(element)}的名称应当为" + ItemName);
            _baseElement = element;
        }

        public DocTitle()
        {
            _baseElement = new XElement(ItemName);
        }
    }
}
