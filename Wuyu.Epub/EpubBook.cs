using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wuyu.Epub.Properties;

namespace Wuyu.Epub
{
    public sealed class EpubBook : IDisposable
    {
        public static readonly XNamespace OpfNs = "http://www.idpf.org/2007/opf";

        public static readonly XNamespace EpubNs = "http://www.idpf.org/2007/ops";

        public static readonly XNamespace DcNs = "http://purl.org/dc/elements/1.1/";

        public static readonly XNamespace XHtmlNs = "http://www.w3.org/1999/xhtml";

        public static readonly XNamespace NcxNs = "http://www.daisy.org/z3986/2005/ncx/";

        public static readonly Dictionary<string, string> MediaType = new()
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

        public Package Package { get; private set; }

        public Nav Nav { get; private set; }

        public Ncx Ncx { get; private set; }

        public string OEBPS { get; private set; }

        public string Opf { get; private set; }

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

        private EpubBook(Stream stream, bool onlyRead = true)
        {
            _epubZip = new ZipArchive(stream, onlyRead ? ZipArchiveMode.Read : ZipArchiveMode.Update);
        }

        public EpubBook()
        {
            _epubZip = new ZipArchive(new MemoryStream(), ZipArchiveMode.Update);
            var mimetype = _epubZip.CreateEntry("mimetype");
            using (var mimetypeStream = mimetype.Open())
            {
                var data = Encoding.UTF8.GetBytes("application/epub+zip");
                mimetypeStream.Write(data, 0, data.Length);
            }

            OEBPS = "OEBPS/";
            Opf = "content.opf";
            Package = new Package();
            Nav = new Nav();
            Language = "zh-CN";
            Title = "[此处填写标题]";
        }

        public static async ValueTask<EpubBook> ReadEpubAsync(Stream inStream, bool onlyRead = true)
        {
            var epub = new EpubBook(inStream, onlyRead);
            var zipArchive = epub._epubZip;

            var container = zipArchive.GetEntry("META-INF/container.xml") ?? throw new FileLoadException("无法解析的文件格式");
            var containerStream = new StreamReader(container.Open());
            var text = await containerStream.ReadToEndAsync();
            var opfPath = Regex.Match(text, "full-path=\"(.*?)\"").Groups[1].Value;
            var index = opfPath.IndexOf('/');
            epub.OEBPS = opfPath[..(index + 1)];
            epub.Opf = opfPath[(index + 1)..];

            var opfEntry = zipArchive.GetEntry(opfPath) ?? throw new FileLoadException("无法解析的文件格式");
            var opfStream = new StreamReader(opfEntry.Open());
            // var opfData = new byte[opfEntry.Length];
            // opfStream.Read(opfData, 0, opfData.Length);
            // var isBom = opfData[0] == 0xef && opfData[1] == 0xbb && opfData[2] == 0xbf;
            var opfText = await opfStream.ReadToEndAsync();
            epub.Package = new Package(opfText);

            var nav = epub.GetNav();
            if (nav != null) epub.Nav = new Nav(epub.GetItemContentByID(nav.ID));

            var ncx = epub.Package.Spine.Toc;
            if (ncx != null)
            {
                epub.Ncx = new Ncx(epub.GetItemContentByID(ncx));
            }

            return epub;
        }

        public static ValueTask<EpubBook> ReadEpubAsync(string filePath, bool onlyRead = true)
        {
            return ReadEpubAsync(new FileStream(filePath, FileMode.Open, onlyRead ? FileAccess.Read : FileAccess.ReadWrite, onlyRead ? FileShare.Read : FileShare.None));
        }

        public static ValueTask<EpubBook> ReadEpubAsync(byte[] data)
        {
            return ReadEpubAsync(new MemoryStream(data));
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

            using (Stream stream = _epubZip.CreateEntry(OEBPS + item.EntryName).Open())
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
                manifestItem.Remove();
                Package.Spine.FirstOrDefault((c) => c.IdRef == manifestItem.ID)?.Remove();
            }
        }

        public Stream GetItemStreamByID(string id)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                return entry?.Open();
            }

            return null;
        }

        public Stream GetItemStreamByHref(string href)
        {
            ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + href);
            return entry?.Open();
        }

        public ZipArchiveEntry GetItemEntryByHref(string href)
        {
            return _epubZip.GetEntry(OEBPS + href);
        }

        public string GetItemContentByID(string id)
        {
            return GetItemContentByIDAsync(id).Result;
        }

        public string GetItemContentByID(string id, Encoding encoding)
        {
            return GetItemContentByIDAsync(id, encoding).Result;
        }

        public async ValueTask<string> GetItemContentByIDAsync(string id)
        {
            return await GetItemContentByIDAsync(id, Encoding.UTF8);
        }

        public async ValueTask<string> GetItemContentByIDAsync(string id, Encoding encoding)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                var stream = entry?.Open();
                if (stream != null)
                {
                    using var streamReader = new StreamReader(stream, encoding);
                    return await streamReader.ReadToEndAsync();
                }
            }
            return null;
        }

        public void SetItemContentByID(string id, string conetnt)
        {
            SetItemContentByIDAsync(id, conetnt).Wait();
        }

        public void SetItemContentByID(string id, string conetnt, Encoding encoding)
        {
            SetItemContentByIDAsync(id, conetnt, encoding).Wait();
        }

        public async Task SetItemContentByIDAsync(string id, string conetnt)
        {
            await SetItemContentByIDAsync(id, conetnt, Encoding.UTF8);
        }

        public async Task SetItemContentByIDAsync(string id, string conetnt, Encoding encoding)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                var stream = entry?.Open();
                if (stream != null)
                {
                    using var sw = new StreamWriter(stream, encoding);
                    stream.SetLength(0);
                    await sw.WriteAsync(conetnt);
                }
            }
        }

        public void SetItemDataByID(string id, byte[] data)
        {
            SetItemDataByIDAsync(id, data).Wait();
        }

        public async Task SetItemDataByIDAsync(string id, byte[] data)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                var stream = entry?.Open();
                if (stream != null)
                {
                    stream.SetLength(0);
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        public async Task<byte[]> ReadStreamAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            byte[] buffer = new byte[81920];
            int read;
            while ((read = await stream.ReadAsync(buffer)) != 0)
            {
                await memoryStream.WriteAsync(buffer.AsMemory(0, read));
            }
            return memoryStream.ToArray();
        }

        public byte[] GetItemDataByID(string id)
        {
            return GetItemDataByIDAsync(id).Result;
        }

        public async ValueTask<byte[]> GetItemDataByIDAsync(string id)
        {
            ManifestItem manifestItem;
            if ((manifestItem = Package.Manifest.SingleOrDefault(i => i.ID == id)) != null)
            {
                ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + manifestItem.Href);
                if (entry == null) return null;
                using var stream = entry.Open();
                return await ReadStreamAsync(stream);
            }
            return null;
        }

        public byte[] GetItemDataByHref(string href)
        {
            return GetItemDataByHrefAsync(href).Result;
        }

        public async ValueTask<byte[]> GetItemDataByHrefAsync(string href)
        {
            ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + href);
            if (entry == null) return null;
            using var stream = entry.Open();
            return await ReadStreamAsync(stream);
        }

        public Stream GetItemStreamByID(string id, out string entryName)
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

        public IEnumerable<ManifestItem> GetItems(IEnumerable<string> extension)
        {
            return from item in Package.Manifest
                   where extension.Contains(Path.GetExtension(item.Href), StringComparer.CurrentCultureIgnoreCase)
                   select item;
        }

        /// <summary>
        /// 返回Epub中可显示内容的迭代对象，按顺序排列
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetTextIDs()
        {
            return Package.Spine.Select(itemRef => itemRef.IdRef);
        }

        /// <summary>
        /// 返回Epub中所有HTML内容的迭代对象，不一定按顺序
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ManifestItem> GetHtmlItems() => GetItems(new[] { ".xhtml", ".html" });

        /// <summary>
        /// EPUB3 独有
        /// </summary>
        /// <returns></returns>
        public ManifestItem GetNav()
        {
            return Package.Manifest.SingleOrDefault(c => c.IsNav);
        }

        /// <summary>
        /// EPUB2 独有
        /// </summary>
        /// <returns></returns>
        public ManifestItem GetNcx()
        {
            if (this.Package.Spine.Toc != null)
            {
                return GetItemById(this.Package.Spine.Toc);
            }
            return null;
        }

        public ManifestItem GetCoverXhtml()
        {
            string href = null;
            if (Version[0] == '3')
            {
                var nav = GetNav();
                if (nav == default) return default;

                var a = Nav.NavGuide.FirstOrDefault(x => x.Type == "cover");
                if (a == default) return default;

                href = Util.ZipResolvePath(Path.GetDirectoryName(nav.Href), a.Href);
            }
            else
            {
                href = Package.Guide.SingleOrDefault(x => x.Type == "cover")?.Href;
            }

            return Package.Manifest.SingleOrDefault(item => item.Href == href);
        }

        public bool SetConverHtml(string href)
        {
            if (Version[0] == '3')
            {
                var nav = GetNav();
                if (nav == default) return false;

                var coverGuide = Nav.NavGuide.FirstOrDefault(x => x.Type == "cover");
                var path = Util.ZipRelativePath(Path.GetDirectoryName(nav.Href), href);
                if (coverGuide == null)
                {
                    Nav.NavGuide.Add(new NavItem { Href = path, Type = "cover", Title = "封面" });
                }
                else
                {
                    coverGuide.Href = path;
                }
            }
            else
            {
                var item = this.Package.Guide.SingleOrDefault(x => x.Href == href);
                if (item == null)
                {
                    this.Package.Guide.Add(new GuideItem { Type = "cover", Title = "封面", Href = href });
                }
                else
                {
                    item.Type = "cover";
                }
            }

            return true;
        }

        public ManifestItem GetItemByHref(string href)
        {
            return Package.Manifest.SingleOrDefault(x => Path.Equals(href, x.Href));
        }

        public ManifestItem GetItemById(string id)
        {
            return Package.Manifest.SingleOrDefault(x => x.ID == id);
        }

        public void AddCoverImage(EpubItem item, bool createHtml = true)
        {
            AddItem(item);
            Package.Manifest.ToList().ForEach(delegate (ManifestItem c) { c.IsCover = item.ID == c.ID; });
            Package.Metadata.Cover = item.ID;
            if (createHtml)
            {
                CreateCoverXhtml(item.ID);
            }
        }

        public void CreateCoverXhtml(string id)
        {
            var href = GetCoverXhtml()?.Href;
            if (href == null) href = "Text/cover.xhtml";
            ZipArchiveEntry val = _epubZip.GetEntry(OEBPS + href);
            if (val == null)
            {
                Package.Manifest.Add(new ManifestItem
                {
                    Href = href,
                    ID = "cover.xhtml",
                    MediaType = MediaType[".xhtml"]
                });
                Package.Spine.Insert(0, new SpineItem("cover.xhtml"));
                val = _epubZip.CreateEntry(OEBPS + "Text/cover.xhtml");
            }

            using (StreamWriter streamWriter = new(val.Open()))
            {
                streamWriter.BaseStream.SetLength(0);
                var item = Package.Manifest.SingleOrDefault(c => c.ID == id);
                streamWriter.Write(string.Format(Resources.cover, Util.ZipRelativePath("Text", item.Href)));
            }

            SetConverHtml(href);
            SetCoverImage(id);
        }

        public string GetEntryName(string id)
        {
            return Package.Manifest.SingleOrDefault(c => c.ID == id)?.Href;
        }

        public void SetCoverImage(string id)
        {
            Package.Manifest.ToList().ForEach(delegate (ManifestItem c) { c.IsCover = id == c.ID; });
            Package.Metadata.Cover = id;
        }

        /// <summary>
        /// 为EPUB3文件生成Nav
        /// </summary>
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
                Package.Manifest.ToList().ForEach(delegate (ManifestItem c) { c.IsNav = c.ID == "nav"; });
            }
            else
            {
                str = manifestItem.Href;
            }

            ZipArchiveEntry entry = _epubZip.GetEntry(OEBPS + str) ?? _epubZip.CreateEntry(OEBPS + str);

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

                using Stream stream = GetItemStreamByID(item, out var entryName);
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

                    list2.Add(Util.ZipRelativePath("Text", entryName));
                }

                stream.SetLength(0L);
                xDocument2.Save(new StreamWriter(stream));
                list.AddRange(xElements);
            }

            XElement xElement2 = null;
            for (var i = 0; i < list.Count; i++)
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

        public void UpdateNav()
        {
            var nav = GetNav();
            if (nav != null)
            {
                var content = GetItemContentByID(nav.ID);
                Nav = new Nav(content);
            }
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
                Package.Manifest.ToList().ForEach(c => c.IsNav = c.ID == "nav");
            }
            else
            {
                str = manifestItem.Href;
            }

            foreach (var item in GetTextIDs())
            {
                if (item == "nav")
                {
                    continue;
                }

                using Stream stream = GetItemStreamByID(item, out var entryName);
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
                    this.Nav.Add(new NavItem { Href = Path.GetFileName(entryName), Title = text });
                }
            }
        }

        private void UpdateZip()
        {
            // 保存Package
            ZipArchiveEntry val = _epubZip.GetEntry(OEBPS + Opf) ?? _epubZip.CreateEntry(OEBPS + Opf);
            using (StreamWriter stream = new(val.Open()))
            {
                stream.BaseStream.SetLength(0);
                Package.Save(stream);
            }

            // 保存Nav
            var nav = this.GetNav();
            if (nav != null)
            {
                var entry = _epubZip.GetEntry(OEBPS + nav.Href);
                using StreamWriter stream = new(entry.Open());
                stream.BaseStream.SetLength(0);
                Nav.Save(stream);
            }

            // todo 保存Ncx
        }

        public async ValueTask SaveAsync(Stream outStream)
        {
            this.UpdateZip();
            using var archive = new ZipArchive(outStream);
            foreach (var item in _epubZip.Entries)
            {
                var entry = archive.CreateEntry(item.Name);
                using var toStream = entry.Open();
                using var inStream = item.Open();
                await inStream.CopyToAsync(toStream);
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing) _epubZip.Dispose();

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}