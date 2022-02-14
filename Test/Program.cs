using System;
using System.IO;
using System.Text;
using Wuyu.Epub;
using System.Xml.Linq;
using AngleSharp.Html.Parser;
using AngleSharp.Xhtml;
using Test.Properties;
using AngleSharp;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //foreach (var filePath in Directory.GetFiles(@"D:\迅雷下载\1\皇帝圣印战记(1)"))
            //{
            //    Console.WriteLine(filePath);
            //    var epub = EpubBook.ReadEpub(File.ReadAllBytes(filePath),
            //        new FileStream(filePath, FileMode.Create), true);
            //    epub.Dispose();
            //}

            //var path = @"D:\迅雷下载\1\天才王子的赤字国家振兴术 05_对了，卖国吧_.epub";
            //var parser = new HtmlParser();
            //var epub = EpubBook.ReadEpub(File.ReadAllBytes(path), new FileStream(path, FileMode.Create));
            //foreach (var item in epub.GetHtmlItems())
            //{
            //    var content = epub.GetItemContentByID(item.ID);
            //    var doc = parser.ParseDocument(content);
            //    foreach (var img in doc.QuerySelectorAll("img"))
            //    {
            //        img.RemoveAttribute("style");
            //    }
            //    epub.SetItemContentByID(item.ID, doc.ToHtml(XhtmlMarkupFormatter.Instance));
            //}
            //epub.Dispose();

            var path = @"D:\迅雷下载\1\漫画家被陌生女高中生监禁的故事.epub";
            var outPaht = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_process.epub");
            var epub = EpubBook.ReadEpub(File.ReadAllBytes(path), new FileStream(outPaht, FileMode.Create), true);

            epub.Dispose();
        }
    }
}