using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class Spine : EpubXElement<SpineItem>, IList<SpineItem>
    {
        // EPub3
        public string Toc
        {
            get => BaseElement.Attribute("toc")?.Value;
            set => BaseElement.SetAttributeValue("toc", value);
        }

        internal Spine(XElement baseElement):base(baseElement)
        {
        }

        public Spine():base(EpubBook.OpfNs + "spine")
        {
        }

        public int IndexOf(SpineItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return BaseElement.Elements(EpubBook.OpfNs + "itemref").ToList().IndexOf(item.BaseElement);
        }

        public void Insert(int index, SpineItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (index == 0)
            {
                BaseElement.AddFirst(item.BaseElement);
            }
            else
            {
                var el = BaseElement.Elements(EpubBook.OpfNs + "itemref").ToArray()[index];
                el.AddBeforeSelf(item.BaseElement);
            }
        }

        public void RemoveAt(int index)
        {
            BaseElement.Elements(EpubBook.OpfNs + "itemref").ToArray()[index].Remove();
        }

        public SpineItem this[int index]
        {
            get => BaseElement.Elements(EpubBook.OpfNs + "itemref").Select(item => new SpineItem(item))
                .ToArray()[index];
            set => BaseElement.Elements(EpubBook.OpfNs + "itemref").ToArray()[index].ReplaceWith(value.BaseElement);
        }
    }
}