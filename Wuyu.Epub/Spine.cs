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

        // EPub3
        public string Toc
        {
            get => BaseElement.Attribute("toc")?.Value;
            set => BaseElement.SetAttributeValue("toc", value);
        }

        internal Spine(XElement baseElement) : base(baseElement)
        {
        }

        public Spine() : base(EpubBook.OpfNs + "spine")
        {
        }
    }
}