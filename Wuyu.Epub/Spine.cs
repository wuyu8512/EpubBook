using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class Spine : EpubXElement<SpineItem>
    {
        protected override XName ItemName { get; } = EpubBook.OpfNs + "itemref";

        // EPub2 льсп
        public string Toc
        {
            get => _baseElement.Attribute("toc")?.Value;
            set => _baseElement.SetAttributeValue("toc", value);
        }

        internal Spine(XElement element) : base(element)
        {
        }

        public Spine() : base(EpubBook.OpfNs + "spine")
        {
        }
    }
}