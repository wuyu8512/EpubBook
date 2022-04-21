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
            var epub = EpubBook.ReadEpub(@"D:\迅雷下载\轻小说-临时\三坪房间的侵略者！？ 36.epub", new MemoryStream());
             
            //var path = @"D:\迅雷下载\1\渴求游戏之神 04.epub";
            //var outPaht = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + "_process.epub");
            //var epub = EpubBook.ReadEpub(File.ReadAllBytes(path), new FileStream(outPaht, FileMode.Create), true);

            //epub.Dispose();
        }
    }
}