using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class Metadata
    {
        public XElement BaseElement { get; }

        public string Identifier
        {
            get => BaseElement.Elements(EpubBook.DcNs + "identifier").FirstOrDefault()?.Value;
            set
            {
                var xElement = BaseElement.Elements(EpubBook.DcNs + "identifier").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "identifier", value, new XAttribute("id", "BookId"));
                    BaseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Title
        {
            get => BaseElement.Elements(EpubBook.DcNs + "title").FirstOrDefault()?.Value;
            set
            {
                var xElement = BaseElement.Elements(EpubBook.DcNs + "title").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "title", value);
                    BaseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Language
        {
            get => BaseElement.Elements(EpubBook.DcNs + "language").FirstOrDefault()?.Value;
            set
            {
                var xElement = BaseElement.Elements(EpubBook.DcNs + "language").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "language", value);
                    BaseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Author
        {
            get => GetCreator().FirstOrDefault()?.Value;
            set
            {
                var xElement = GetCreator().FirstOrDefault();
                if (xElement == null)
                {
                    BaseElement.AddFirst(new XElement(EpubBook.DcNs + "creator", value, new XAttribute("id", "cre")),
                        new XElement(EpubBook.OpfNs + "meta", "aut", new XAttribute("refines", "#cre"),
                            new XAttribute("property", "role")));
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Creator
        {
            get =>
                (from e in BaseElement.Elements(EpubBook.DcNs + "creator")
                    where e.Attribute("role")?.Value == null
                    select e).FirstOrDefault()?.Value;
            set
            {
                var xElement = (from e in BaseElement.Elements(EpubBook.DcNs + "creator")
                    where e.Attribute("role")?.Value == null
                    select e).FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "creator", value);
                    BaseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Cover
        {
            get =>
                (from e in BaseElement.Elements(EpubBook.OpfNs + "meta")
                    where e.Attribute("name")?.Value == "cover"
                    select e).FirstOrDefault()?.Attribute("content")?.Value;
            set
            {
                var xElement = (from e in BaseElement.Elements(EpubBook.OpfNs + "meta")
                    where e.Attribute("name")?.Value == "cover"
                    select e).FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.OpfNs + "meta", new XAttribute("name", "cover"),
                        new XAttribute("content", value));
                    BaseElement.Add(xElement);
                }
                else
                {
                    xElement.SetAttributeValue("content", value);
                }
            }
        }

        public Metadata(XElement element)
        {
            BaseElement = element;
        }

        public Metadata()
        {
            BaseElement = new XElement(EpubBook.OpfNs + "metadata",
                new XAttribute(XNamespace.Xmlns + "dc", EpubBook.DcNs.NamespaceName));
        }

        public IEnumerable<XElement> GetMetaDataItem(XName name)
        {
            return BaseElement.Elements(name);
        }

        private IEnumerable<XElement> GetCreator()
        {
            return from creator in BaseElement.Elements(EpubBook.DcNs + "creator")
                join meta in BaseElement.Elements(EpubBook.OpfNs + "meta") on "#" + creator.Attribute("id")?.Value
                    equals meta.Attribute("refines")?.Value
                where meta.Value == "aut" && meta.Attribute("property")?.Value == "role"
                select creator;
        }

        public void AddMetaDataItem(XName name, string content, object attributes = null)
        {
            var xElement = new XElement(name, content);
            if (attributes != null)
            {
                var properties = attributes.GetType().GetProperties();
                foreach (var propertyInfo in properties)
                {
                    var value = propertyInfo.GetValue(attributes);
                    if (value != null) xElement.Add(new XAttribute(propertyInfo.Name, value));
                }
            }

            BaseElement.Add(xElement);
        }
    }
}