using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wuyu.Epub.NcxItem
{
    public class NavPoint : EpubXElementItem
    {
        protected override XName ItemName => EpubBook.NcxNs + "navPoint";

        public string ID
        {
            get => _baseElement.Attribute("id")?.Value;
            set => _baseElement.SetAttributeValue("id", value);
        }

        public string PlayOrder
        {
            get => _baseElement.Attribute("playOrder")?.Value;
            set => _baseElement.SetAttributeValue("playOrder", value);
        }

        public string Src
        {
            get => _baseElement.Element(EpubBook.NcxNs + "content")?.Attribute("src")?.Value;
            set
            {
                var content = _baseElement.Element(EpubBook.NcxNs + "content");
                if (content == null)
                {
                    content = new XElement(EpubBook.NcxNs + "content");
                    _baseElement.Add(content);
                }
                content.SetAttributeValue("src", value);
            }
        }

        public string Text
        {
            get => _baseElement.Element(EpubBook.NcxNs + "navLabel")?.Element(EpubBook.NcxNs + "text")?.Value;
            set
            {
                var lavel = _baseElement.Element(EpubBook.NcxNs + "navLabel");
                if (lavel == null)
                {
                    lavel = new XElement(EpubBook.NcxNs + "content", new XElement(EpubBook.NcxNs + "text"));
                    _baseElement.Add(lavel);
                }
                // todo text 可能不存在
                lavel.Element(EpubBook.NcxNs + "text").Value = value;
            }
        }

        public NavPoint(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException($"{nameof(element)}的名称应当为" + ItemName);
            _baseElement = element;
        }

        public NavPoint()
        {
            _baseElement = new XElement(ItemName);
        }
    }
}
