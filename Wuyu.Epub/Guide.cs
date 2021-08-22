using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Wuyu.Epub
{
    public class Guide : EpubXElement<GuideItem>
    {
        protected override XName ItemName => EpubBook.OpfNs + "reference";

        internal Guide(XElement baseElement) : base(baseElement)
        {
        }

        public Guide() : base(EpubBook.OpfNs + "guide")
        {
        }
    }
}