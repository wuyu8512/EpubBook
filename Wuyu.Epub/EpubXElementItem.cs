using System.Xml.Linq;

namespace Wuyu.Epub
{
    public abstract class EpubXElementItem
    {
        public XElement BaseElement { get; protected set; }
    }
}