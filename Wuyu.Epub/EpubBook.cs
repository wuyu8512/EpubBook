using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Wuyu.Epub.Properties;

namespace Wuyu.Epub
{
    public sealed class EpubBook : IDisposable
    {
        public static readonly XNamespace OpfNs = "http://www.idpf.org/2007/opf";

        public static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

        public static readonly XNamespace XHtmlNs = "http://www.w3.org/1999/xhtml";

        public static readonly Dictionary<string, string> MediaType = new Dictionary<string, string>
        {
            {
                ".jpeg",
                "image/jpeg"
            },
            {
                ".jpg",
                "image/jpeg"
            },
            {
                ".png",
                "image/png"
            },
            {
                ".gif",
                "image/gif"
            },
            {
                ".webp",
                "image/webp"
            },
            {
                ".bmp",
                "image/bmp"
            },
            {
                ".xhtml",
                "application/xhtml+xml"
            },
            {
                ".html",
                "application/xhtml+xml"
            },
            {
                ".css",
                "text/css"
            },
            {
                ".ttf",
                "application/x-font-ttf"
            }
        };

        private readonly ZipArchive _epubZip;

        private Package Package;

        private string OEBPS;

        private string Opf;

        private bool _disposedValue;

        public string Version
        {
            get => Package.Version;
            set => Package.Version = value;
        }

        public string Identifier
        {
            get => Package.Metadata.Identifier;
            set => Package.Metadata.Identifier = value;
        }

        public string Title
        {
            get => Package.Metadata.Title;
            set => Package.Metadata.Title = value;
        }

        public string Creator
        {
            get => Package.Metadata.Creator;
            set => Package.Metadata.Creator = value;
        }

        public string Author
        {
            get => Package.Metadata.Author;
            set => Package.Metadata.Author = value;
        }

        public string Language
        {
            get => Package.Metadata.Language;
            set => Package.Metadata.Language = value;
        }

        public string Cover
        {
            get => Package.Metadata.Cover;
            set => Package.Metadata.Cover = value;
        }

        public EpubBook(Stream stream)
        {
            _epubZip = new ZipArchive(stream, (ZipArchiveMode) 2);
            var mimetype = _epubZip.CreateEntry("mimetype",CompressionLevel.Optimal);
            using (var mimetypeStream = mimetype.Open())
            {
                var data = Encoding.UTF8.GetBytes("application/epub+zip");
                mimetypeStream.Write(data, 0, data.Length);
            }

            OEBPS = "OEBPS/";
            Opf = "content.opf";
            Package = new Package();
            Language = "zh-CN";
            Title = "[此处填写标题]";
        }

        public static EpubBook ReadEpub(Stream inStream, Stream outStream, bool compatibleMode = false)
        {
            ZipArchive zipArchive = new ZipArchive(inStream);
            EpubBook epub = new EpubBook(outStream);
            var container = zipArchive.GetEntry("META-INF/container.xml");
            if (container == null) throw new FileLoadException("无法解析的文件格式");
            using (var stream = container.Open())
            {
                var data = new byte[container.Length];
                stream.Read(data, 0, data.Length);
                var text = Encoding.UTF8.GetString(data);
                //full-path="OPS/fb.opf"
                var opfPath = Regex.Match(text, "full-path=\"(.*?)\"").Groups[1].Value;
                var index = opfPath.IndexOf('/');
                epub.OEBPS = opfPath[..(index + 1)];
                epub.Opf = opfPath[(index + 1)..];

                var opfEntry = zipArchive.GetEntry(opfPath);
                if (opfEntry == null) throw new FileLoadException("无法解析的文件格式");
                using (var opfStream = opfEntry.Open())
                {
                    var opfData = new byte[opfEntry.Length];
                    opfStream.Read(opfData, 0, opfData.Length);
                    var isBom = opfData[0] == 0xef && opfData[1] == 0xbb && opfData[2] == 0xbf;
                    var opfText = Encoding.UTF8.GetString(isBom ? opfData[3..] : opfData);
                    epub.Package = new Package(opfText);
                }

                List<XElement> dl = new List<XElement>();

                foreach (ManifestItem item in epub.Package.Manifest)
                {
                    var entryName = epub.OEBPS + item.Href;
                    var entry = zipArchive.GetEntry(entryName)?.Open();
                    if (entry == null)
                    {
                        Console.WriteLine($"[EPubRead]没有{entryName}这个文件");
                        dl.Add(item.BaseElement);
                        continue;
                    }
                    using (Stream newEntry = epub._epubZip.CreateEntry(entryName,CompressionLevel.Optimal).Open())
                    {
                        entry.CopyTo(newEntry);
                    }
                }

                foreach (var xElement in dl) xElement.Remove();

                if (compatibleMode)
                {
                    foreach (var entry in zipArchive.Entries)
                    {
                        var href = entry.FullName.Replace(epub.OEBPS, "");
                        if (entry.FullName != opfPath && !string.IsNullOrEmpty(entry.Name) &&
                            entry.FullName.Contains(epub.OEBPS) &&
                            !epub.HashItemByBaseName(href))
                        {
                            epub.Package.Manifest.Add(new ManifestItem(entry.Name, href));
                            using (Stream newEntry = epub._epubZip.CreateEntry(entry.FullName,CompressionLevel.Optimal).Open())
                            {
                                entry.Open().CopyTo(newEntry);
                            }
                        }
                    }
                }
            }

            return epub;
        }

        public static EpubBook ReadEpub(string filePath, Stream outStream, bool compatibleMode = false)
        {
            return ReadEpub(new FileStream(filePath, FileMode.Open), outStream, compatibleMode);
        }
        
        public static EpubBook ReadEpub(byte[] data, Stream outStream, bool compatibleMode = false)
        {
            return ReadEpub(new MemoryStream(data), outStream, compatibleMode);
        }
        
        public bool HasItemByID(string id)
        {
            return Package.Manifest.Any(item => item.ID == id);
        }

        public bool HashItemByBaseName(string baseName)
        {
            return Package.Manifest.Any(item => item.Href == baseName);
        }

        public void AddItem(EpubItem item)
        {
            if (string.IsNullOrWhiteSpace(item.ID) || string.IsNullOrWhiteSpace(item.EntryName))
            {
                throw new ArgumentException("ID和BaseName不能为空");
            }

            if (HasItemByID(item.ID) || HashItemByBaseName(item.EntryName))
            {
                throw new ArgumentException("ID和BaseName必须唯一");
            }

            var extension = Path.GetExtension(item.EntryName);
            if (extension == null || !MediaType.ContainsKey(extension))
                throw new ArgumentException("不支持的文件类型:" + item.EntryName);
            Package.Manifest.Add(new ManifestItem
            {
                ID = item.ID,
                Href = item.EntryName,
                MediaType = MediaType[extension]
            });
            if (extension == ".xhtml" && item.ID != "nav")
            {
                Package.Spine.Add(new SpineItem(item.ID));
            }

            using (Stream stream = _epubZip.CreateEntry(OEBPS + item.EntryName,CompressionLevel.Optimal).Open())
            {
                stream.Write(item.Data, 0, item.Data.Length);
            }
        }

        public void DeleteItem(string id)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry;
                while ((entry = _epubZip.GetEntry(OEBPS + manifestItem.Href)) != null)
                {
                    entry.Delete();
                }
            }
        }

        public Stream GetItemByID(string id)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                return entry?.Open();
            }

            return null;
        }

        public Stream GetItemByID(string id, out string entryName)
        {
            entryName = null;
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                entryName = manifestItem.Href;
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                return entry?.Open();
            }

            return null;
        }

        public IEnumerable<string> GetItemIDs(IEnumerable<string> extension)
        {
            return from item in Package.Manifest where extension.Contains(Path.GetExtension(item.Href)) select item.ID;
        }

        public IEnumerable<string> GetTextIDs()
        {
            return Package.Spine.Select(itemRef => itemRef.IdRef);
        }

        public void AddCoverImage(EpubItem item, bool createHtml = true)
        {
            AddItem(item);
            Package.Manifest.ToList().ForEach(delegate(ManifestItem c) { c.IsCover = item.ID == c.ID; });
            Package.Metadata.Cover = item.ID;
            if (createHtml)
            {
                CreateCoverXhtml(item.ID);
            }
        }

        public void CreateCoverXhtml(string id)
        {
            ZipArchiveEntry val = _epubZip.GetEntry(OEBPS + "Text/cover.xhtml");
            if (val == null)
            {
                Package.Manifest.Add(new ManifestItem
                {
                    Href = "Text/cover.xhtml",
                    ID = "cover.xhtml",
                    MediaType = MediaType[".xhtml"]
                });
                Package.Spine.Insert(0, new SpineItem() {IdRef = "cover.xhtml"});
                val = _epubZip.CreateEntry(OEBPS + "Text/cover.xhtml",CompressionLevel.Optimal);
            }

            using (StreamWriter streamWriter = new StreamWriter(val.Open()))
            {
                string entryName;
                using Stream stream = GetItemByID(id, out entryName);
                streamWriter.Write(string.Format(Resources.cover, entryName));
            }

            SetCoverImage(id);
        }

        public string GetEntryName(string id)
        {
            return Package.Manifest.SingleOrDefault(c => c.ID == id)?.Href;
        }

        public void SetCoverImage(string id)
        {
            Package.Manifest.ToList().ForEach(delegate(ManifestItem c) { c.IsCover = id == c.ID; });
            Package.Metadata.Cover = id;
        }

        public void AddNav()
        {
            ManifestItem manifestItem = Package.Manifest.SingleOrDefault(c => c.IsNav);
            string str;
            if (manifestItem == null)
            {
                AddItem(new EpubItem
                {
                    EntryName = "Text/nav.xhtml",
                    ID = "nav",
                    Data = Array.Empty<byte>()
                });
                str = "Text/nav.xhtml";
                Package.Manifest.ToList().ForEach(delegate(ManifestItem c) { c.IsNav = c.ID == "nav"; });
            }
            else
            {
                str = manifestItem.Href;
            }

            ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + str) ?? _epubZip.CreateEntry(OEBPS + str,CompressionLevel.Optimal);

            using StreamWriter textWriter = new StreamWriter(entry.Open());
            XDocument xDocument = XDocument.Parse(Resources.nav);
            XElement xElement = xDocument.Root.Descendants(XHtmlNs + "ol").First();
            var list = new List<XElement>();
            var hName = new string[6]
            {
                "h1",
                "h2",
                "h3",
                "h4",
                "h5",
                "h6"
            };
            var num = 0;
            var list2 = new List<string>();
            var list3 = new List<string>();
            foreach (var item in GetTextIDs())
            {
                if (item == "nav")
                {
                    continue;
                }

                using Stream stream = GetItemByID(item, out var entryName);
                XDocument xDocument2 = XDocument.Load(stream);
                var xElements = xDocument2.Descendants().Where(element => hName.Contains(element.Name.LocalName))
                    .ToArray();
                foreach (XElement item2 in xElements)
                {
                    XAttribute xAttribute = item2.Attribute("id");
                    if (xAttribute == null)
                    {
                        var text = "ebooklib_id_" + num++;
                        item2.Add(new XAttribute("id", text));
                        list3.Add(text);
                    }
                    else
                    {
                        list3.Add(xAttribute.Value);
                    }

                    // Todo 需要的是一个相对位置
                    list2.Add(entryName);
                }

                stream.SetLength(0L);
                xDocument2.Save(new StreamWriter(stream));
                list.AddRange(xElements);
            }

            XElement xElement2 = null;
            for (var i = 0; i < list.Count(); i++)
            {
                if (i == 0)
                {
                    xElement2 = new XElement(XHtmlNs + "li",
                        new XElement(XHtmlNs + "a", list[0].Value, new XAttribute("href", list2[0] + "#" + list3[0])));
                    xElement.Add(xElement2, new XAttribute("level", list[i].Name.LocalName));
                }
                else
                {
                    xElement2 = Funb(xElement2, list[i].Name.LocalName, list[i].Value, list2[i] + "#" + list3[i]);
                }
            }

            foreach (XElement item3 in xDocument.Descendants(XHtmlNs + "ol"))
            {
                item3.Attribute("level")?.Remove();
            }

            xDocument.Save(textWriter);
        }

        private XElement Funb(XContainer element, string level, string content, string href)
        {
            var num = Compare(element.Parent, level);
            if (num < 0)
            {
                XElement xElement = element.Elements(XHtmlNs + "ol").FirstOrDefault();
                if (xElement == null)
                {
                    xElement = new XElement(XHtmlNs + "ol");
                    element.Add(xElement);
                }

                XElement xElement2 = new XElement(XHtmlNs + "li",
                    new XElement(XHtmlNs + "a", content, new XAttribute("href", href)));
                xElement.Add(xElement2, new XAttribute("level", level));
                return xElement2;
            }

            if (num == 0)
            {
                XElement xElement3 = new XElement(XHtmlNs + "li",
                    new XElement(XHtmlNs + "a", content, new XAttribute("href", href)));
                element.AddAfterSelf(xElement3);
                return xElement3;
            }

            return Funb(element.Parent.Parent, level, content, href);
        }

        private int Compare(XElement element1, string level)
        {
            return string.Compare(element1.Attribute("level").Value, level);
        }

        public void AddNavByTitle()
        {
            ManifestItem manifestItem = Package.Manifest.SingleOrDefault(c => c.IsNav);
            string str;
            if (manifestItem == null)
            {
                AddItem(new EpubItem
                {
                    EntryName = "Text/nav.xhtml",
                    ID = "nav",
                    Data = Array.Empty<byte>()
                });
                str = "Text/nav.xhtml";
                Package.Manifest.ToList().ForEach(delegate(ManifestItem c) { c.IsNav = c.ID == "nav"; });
            }
            else
            {
                str = manifestItem.Href;
            }

            ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + str) ?? _epubZip.CreateEntry(OEBPS + str,CompressionLevel.Optimal);

            using StreamWriter textWriter = new StreamWriter(entry.Open());
            XDocument xDocument = XDocument.Parse(Resources.nav);
            XElement xElement = xDocument.Root.Descendants(XHtmlNs + "ol").First();
            foreach (var item in GetTextIDs())
            {
                if (item == "nav")
                {
                    continue;
                }

                using Stream stream = GetItemByID(item, out var entryName);
                XElement xElement2 = XElement.Load(stream);
                XNamespace defaultNamespace = xElement2.GetDefaultNamespace();
                var text = xElement2.Descendants(defaultNamespace + "title").FirstOrDefault()?.Value;
                var source = new string[2]
                {
                    "目录",
                    "封面"
                };
                if (!string.IsNullOrWhiteSpace(text) && !source.Contains(text))
                {
                    xElement.Add(new XElement(defaultNamespace + "li",
                        new XElement(defaultNamespace + "a", text,
                            new XAttribute("href", Path.GetFileName(entryName)))));
                }
            }

            xDocument.Save((TextWriter) textWriter);
        }

        public IEnumerable<XElement> GetMetadata(XName name)
        {
            return Package.Metadata.GetMetaDataItem(name);
        }

        public void AddMetaDataItem(XName name, string content, object attributes = null)
        {
            Package.Metadata.AddMetaDataItem(name, content, attributes);
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }

            if (disposing)
            {
                ZipArchiveEntry val = _epubZip.GetEntry(OEBPS + Opf);
                if (val == null)
                {
                    val = _epubZip.CreateEntry(OEBPS + Opf,CompressionLevel.Optimal);
                }

                using (StreamWriter stream = new StreamWriter(val.Open()))
                {
                    Package.Save(stream);
                }

                foreach (var item in GetItemIDs(new string[2]
                {
                    ".xhtml",
                    ".html"
                }))
                {
                    using Stream stream2 = GetItemByID(item);
                    var array = new byte[stream2.Length];
                    stream2.Read(array, 0, array.Length);
                    var @string = Encoding.UTF8.GetString(array);
                    @string = @string.Replace(" []>", ">");
                    @string = @string.Replace("[]>", ">");
                    stream2.SetLength(0L);
                    array = Encoding.UTF8.GetBytes(@string);
                    stream2.Write(array, 0, array.Length);
                }

                var container = _epubZip.CreateEntry("META-INF/container.xml",CompressionLevel.Optimal);
                using (var containerStream = container.Open())
                {
                    var data = Encoding.UTF8.GetBytes(string.Format(Resources.container, OEBPS + Opf));
                    containerStream.Write(data, 0, data.Length);
                }

                _epubZip.Dispose();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}