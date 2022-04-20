using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Wuyu.Epub.Properties;

namespace Wuyu.Epub
{
    public class Package
    {
        public Metadata Metadata { get; private set; }

        public Spine Spine { get; private set; }

        public Manifest Manifest { get; private set; }

        public Guide Guide { get; private set; }

        public XElement BaseElement { get; private set; }

        public string Version
        {
            get => BaseElement.Attribute("version")?.Value;
            set => BaseElement.SetAttributeValue("version", value);
        }

        public Package(string content)
        {
            var doc = XDocument.Parse(content);
            BaseElement = doc.Root;
            if (BaseElement == null) throw new ArgumentException("无法识别的内容");

            // Metadata
            var xElement = BaseElement.Elements(EpubBook.OpfNs + "metadata").FirstOrDefault();
            if (xElement == null)
            {
                Metadata = new Metadata();
                BaseElement.Add(Metadata.BaseElement);
            }
            else Metadata = new Metadata(xElement);

            // Manifest
            xElement = BaseElement.Elements(EpubBook.OpfNs + "manifest").FirstOrDefault();
            if (xElement == null)
            {
                Manifest = new Manifest();
                BaseElement.Add(Manifest.BaseElement);
            }
            else Manifest = new Manifest(xElement);

            // Spine
            xElement = BaseElement.Elements(EpubBook.OpfNs + "spine").FirstOrDefault();
            if (xElement == null)
            {
                Spine = new Spine();
                BaseElement.Add(Spine.BaseElement);
            }
            else Spine = new Spine(xElement);

            //Guide
            xElement = BaseElement.Elements(EpubBook.OpfNs + "guide").FirstOrDefault();
            if (xElement == null)
            {
                Guide = new Guide();
                BaseElement.Add(Guide.BaseElement);
            }
            else Guide = new Guide(xElement);
        }

        public Package() : this(Resources.package)
        {
            Metadata.Creator = "无语";
            Metadata.Identifier = "urn:uuid:" + Guid.NewGuid().ToString("D");
        }

        public void Save(TextWriter stream)
        {
            var xElement = Metadata.BaseElement.Element(EpubBook.DcNs + "rights");
            if (xElement == null) Metadata.AddMetaDataItem(EpubBook.DcNs + "rights", "此Epub由Wuyu.Epub生成");
            else xElement.Value = "此Epub由Wuyu.Epub生成";
            BaseElement.Save(stream);
        }
    }
}