using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public abstract class EpubXElementItem
    {
        public XElement BaseElement { get; protected set; }

        public void Remove() => BaseElement.Remove();

        protected abstract XName ItemName { get; }
    }
}