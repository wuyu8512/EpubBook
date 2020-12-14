using System;
using System.IO;
using System.Text;
using Wuyu.Epub;
using System.Xml.Linq;
using Test.Properties;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //EpubBook epub = new EpubBook(new FileStream(@"F:\EPUB\程序测试\1.epub", FileMode.Create));
            //epub.AddItem(new EpubItem() { Data = Encoding.UTF8.GetBytes(File.ReadAllText(@"D:\迅雷下载\我与她的绝对领域\我与她的绝对领域_1\OPS\chapter2.html")), EntryName = "Text/1.xhtml", ID = "1.xhtml" });
            //epub.AddNav();
            //epub.Dispose();

            var epub = EpubBook.ReadEpub(@"/media/硬盘/文档/迅雷下载/白银龙王的摇篮/白银龙王的摇篮 02.epub",
                new FileStream(@"/media/硬盘/办公/EPUB/程序测试/白银龙王的摇篮 02_修改.epub", FileMode.Create), true);
            epub.Dispose();

            // MemoryStream memoryStream = new MemoryStream();
            // StreamWriter stream = new StreamWriter(memoryStream);
            //
            // var package2 = new Package(Resources.package);
            //
            // Package package = new Package();
            // package.Guide.Add(new GuideItem(){Type = "cover",Title = "cover",Href = "Text/cover.xhtml"});
            // package.Manifest.Add(new ManifestItem("cover","Text/Cover.xhtml"));
            // package.Spine.Add(new SpineItem("cover"));
            // package.Metadata.Creator = "wuyu";
            // package.Metadata.Title = "wuyu";
            //
            // package.Save(stream);
            // stream.Dispose();
            // var data = memoryStream.ToArray();
            // var a = Encoding.UTF8.GetString(data);
        }
    }
}