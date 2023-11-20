using System;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public abstract class EpubXElementItem
    {
        internal XElement _baseElement;

        public void Remove() => _baseElement.Remove();

        protected abstract XName ItemName { get; }
    }
}