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
            get => BaseElement.Element(EpubBook.NcxNs + "text")?.Value;
            set
            {
                var textElement = BaseElement.Element(EpubBook.NcxNs + "text");
                if (textElement == null)
                {
                    textElement = new XElement(EpubBook.NcxNs + "text");
                    BaseElement.Add(textElement);
                }
                textElement.Value = value;
            }
        }

        public DocTitle(XElement baseElement)
        {
            if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            BaseElement = baseElement;
        }

        public DocTitle()
        {
            BaseElement = new XElement(ItemName);
        }
    }
}
