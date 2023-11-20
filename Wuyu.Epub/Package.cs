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

        internal XElement _baseElement;

        public string Version
        {
            get => _baseElement.Attribute("version")?.Value;
            set => _baseElement.SetAttributeValue("version", value);
        }

        public Package(string content)
        {
            var doc = XDocument.Parse(content);
            _baseElement = doc.Root;
            if (_baseElement == null) throw new ArgumentException("无法识别的内容");

            // Metadata
            var xElement = _baseElement.Elements(EpubBook.OpfNs + "metadata").FirstOrDefault();
            if (xElement == null)
            {
                Metadata = new Metadata();
                _baseElement.Add(Metadata._baseElement);
            }
            else Metadata = new Metadata(xElement);

            // Manifest
            xElement = _baseElement.Elements(EpubBook.OpfNs + "manifest").FirstOrDefault();
            if (xElement == null)
            {
                Manifest = new Manifest();
                _baseElement.Add(Manifest._baseElement);
            }
            else Manifest = new Manifest(xElement);

            // Spine
            xElement = _baseElement.Elements(EpubBook.OpfNs + "spine").FirstOrDefault();
            if (xElement == null)
            {
                Spine = new Spine();
                _baseElement.Add(Spine._baseElement);
            }
            else Spine = new Spine(xElement);

            //Guide
            xElement = _baseElement.Elements(EpubBook.OpfNs + "guide").FirstOrDefault();
            if (xElement == null)
            {
                Guide = new Guide();
                _baseElement.Add(Guide._baseElement);
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
            var xElement = Metadata._baseElement.Element(EpubBook.DcNs + "rights");
            if (xElement == null) Metadata.AddMetaDataItem(EpubBook.DcNs + "rights", "此Epub由Wuyu.Epub生成");
            else xElement.Value = "此Epub由Wuyu.Epub生成";
            _baseElement.Save(stream);
        }
    }
}