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
            get => _baseElement.Attribute("id")?.Value;
            set => _baseElement.SetAttributeValue("id", value);
        }

        public string Href
        {
            get => _baseElement.Attribute("href")?.Value;
            set => _baseElement.SetAttributeValue("href", value);
        }

        public string MediaType
        {
            get => _baseElement.Attribute("media-type")?.Value;
            set => _baseElement.SetAttributeValue("media-type", value);
        }

        // EPub3
        public bool IsCover
        {
            get => _baseElement.Attribute("properties")?.Value == "cover-image";
            set
            {
                if (IsCover)
                {
                    _baseElement.SetAttributeValue("properties", "cover-image");
                }
            }
        }

        // EPub3
        public bool IsNav
        {
            get => _baseElement.Attribute("properties")?.Value == "nav";
            set
            {
                if (value)
                {
                    _baseElement.SetAttributeValue("properties", "nav");
                }
            }
        }

        public ManifestItem()
        {
            _baseElement = new XElement(ItemName);
        }

        public ManifestItem(string id, string href)
        {
            _baseElement = new XElement(ItemName);
            ID = id;
            Href = href;
            MediaType = EpubBook.MediaType[Path.GetExtension(href)];
        }

        public ManifestItem(XElement element)
        {
            if (element.Name != ItemName) throw new ArgumentException($"{nameof(element)}的名称应当为" + ItemName);
            _baseElement = element;
        }
    }
}