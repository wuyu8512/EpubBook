using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class NavPoint : EpubXElement<NavPoint>
    {
        protected override XName ItemName => EpubBook.NcxNs + "navPoint";

        public string ID
        {
            get => BaseElement.Attribute("id")?.Value;
            set => BaseElement.SetAttributeValue("id", value);
        }

        public string PlayOrder
        {
            get => BaseElement.Attribute("playOrder")?.Value;
            set => BaseElement.SetAttributeValue("playOrder", value);
        }

        public string Src
        {
            get => BaseElement.Element(EpubBook.NcxNs + "content")?.Attribute("src")?.Value;
            set
            {
                var content = BaseElement.Element(EpubBook.NcxNs + "content");
                if (content == null)
                {
                    content = new XElement(EpubBook.NcxNs + "content");
                    BaseElement.Add(content);
                }
                content.SetAttributeValue("src", value);
            }
        }

        public string Text
        {
            get => BaseElement.Element(EpubBook.NcxNs + "navLabel")?.Element(EpubBook.NcxNs + "text")?.Value;
            set
            {
                var lavel = BaseElement.Element(EpubBook.NcxNs + "navLabel");
                if (lavel == null)
                {
                    lavel = new XElement(EpubBook.NcxNs + "content", new XElement(EpubBook.NcxNs + "text"));
                    BaseElement.Add(lavel);
                }
                // todo text 可能不存在
                lavel.Element(EpubBook.NcxNs + "text").Value = value;
            }
        }

        public NavPoint(XElement baseElement) : base(baseElement)
        {
            //if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            //BaseElement = baseElement;
        }

        public NavPoint() : base(EpubBook.NcxNs + "navPoint")
        {
            //BaseElement = new XElement(ItemName);
        }
    }
}
