using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Wuyu.Epub
{
	public class Manifest: EpubXElement<ManifestItem>
	{
		public Manifest():base(EpubBook.OpfNs + "manifest")
		{
		}

		internal Manifest(XElement element):base(element)
		{
		}
	}
}
