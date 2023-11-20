using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class Metadata
    {
        internal XElement _baseElement { get; }

        public string Identifier
        {
            get => _baseElement.Elements(EpubBook.DcNs + "identifier").FirstOrDefault()?.Value;
            set
            {
                var xElement = _baseElement.Elements(EpubBook.DcNs + "identifier").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "identifier", value, new XAttribute("id", "BookId"));
                    _baseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Title
        {
            get => _baseElement.Elements(EpubBook.DcNs + "title").FirstOrDefault()?.Value;
            set
            {
                var xElement = _baseElement.Elements(EpubBook.DcNs + "title").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "title", value);
                    _baseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Language
        {
            get => _baseElement.Elements(EpubBook.DcNs + "language").FirstOrDefault()?.Value;
            set
            {
                var xElement = _baseElement.Elements(EpubBook.DcNs + "language").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "language", value);
                    _baseElement.AddFirst(xElement);
                }
                else
                {
                    xElement.Value = value;
                }
            }
        }

        public string Author
        {
            get => GetAuthor().FirstOrDefault()?.Value;
            set
            {
                var xElement = GetAuthor().FirstOrDefault();
                if (xElement == null)
                {
                    _baseElement.AddFirst(new XElement(EpubBook.DcNs + "creator", value, new XAttribute("id", "cre")),
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
            get => GetCreator().FirstOrDefault()?.Value;
            set
            {
                var xElement = GetCreator().FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.DcNs + "creator", value);
                    _baseElement.AddFirst(xElement);
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
                (from e in _baseElement.Elements(EpubBook.OpfNs + "meta")
                    where e.Attribute("name")?.Value == "cover"
                    select e).FirstOrDefault()?.Attribute("content")?.Value;
            set
            {
                var xElement = (from e in _baseElement.Elements(EpubBook.OpfNs + "meta")
                    where e.Attribute("name")?.Value == "cover"
                    select e).FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(EpubBook.OpfNs + "meta", new XAttribute("name", "cover"),
                        new XAttribute("content", value));
                    _baseElement.Add(xElement);
                }
                else
                {
                    xElement.SetAttributeValue("content", value);
                }
            }
        }

        public Metadata(XElement element)
        {
            _baseElement = element;
        }

        public Metadata()
        {
            _baseElement = new XElement(EpubBook.OpfNs + "metadata",
                new XAttribute(XNamespace.Xmlns + "dc", EpubBook.DcNs.NamespaceName));
        }

        public IEnumerable<XElement> GetMetaDataItem(XName name)
        {
            return _baseElement.Elements(name);
        }

        private IEnumerable<XElement> GetAuthor()
        {
            return from creator in _baseElement.Elements(EpubBook.DcNs + "creator")
                join meta in _baseElement.Elements(EpubBook.OpfNs + "meta") on "#" + creator.Attribute("id")?.Value
                    equals meta.Attribute("refines")?.Value
                where meta.Value == "aut" && meta.Attribute("property")?.Value == "role"
                select creator;
        }

        private IEnumerable<XElement> GetCreator()
        {
            return from creator in _baseElement.Elements(EpubBook.DcNs + "creator")
                   join meta in _baseElement.Elements(EpubBook.OpfNs + "meta") 
                   on "#" + creator.Attribute("id")?.Value equals meta.Attribute("refines")?.Value into metaGroup
                   from meta in metaGroup.DefaultIfEmpty()
                   where meta?.Value != "aut"
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

            _baseElement.Add(xElement);
        }
    }
}