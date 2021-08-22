using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

namespace Wuyu.Epub
{
    public class ManifestItem : EpubXElementItem
    {
        protected override XName ItemName { get; } = EpubBook.OpfNs + "item";

        public string ID
        {
            get => BaseElement.Attribute("id")?.Value;
            set => BaseElement.SetAttributeValue("id", value);
        }

        public string Href
        {
            get => BaseElement.Attribute("href")?.Value;
            set => BaseElement.SetAttributeValue("href", value);
        }

        public string MediaType
        {
            get => BaseElement.Attribute("media-type")?.Value;
            set => BaseElement.SetAttributeValue("media-type", value);
        }

        // EPub3
        public bool IsCover
        {
            get => BaseElement.Attribute("properties")?.Value == "cover-image";
            set
            {
                if (IsCover)
                {
                    BaseElement.SetAttributeValue("properties", "cover-image");
                }
            }
        }

        // EPub3
        public bool IsNav
        {
            get => BaseElement.Attribute("properties")?.Value == "nav";
            set
            {
                if (value)
                {
                    BaseElement.SetAttributeValue("properties", "nav");
                }
            }
        }

        public ManifestItem()
        {
            BaseElement = new XElement(ItemName);
        }

        public ManifestItem(string id, string href)
        {
            BaseElement = new XElement(ItemName);
            ID = id;
            Href = href;
            MediaType = EpubBook.MediaType[Path.GetExtension(href)];
        }

        public ManifestItem(XElement baseElement)
        {
            if (baseElement.Name != ItemName) throw new ArgumentException("baseElement的名称应当为" + ItemName);
            BaseElement = baseElement;
        }
    }
}