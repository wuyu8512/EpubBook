using System;
using System.IO;
using System.Text;
using Wuyu.Epub;
using System.Xml.Linq;
using AngleSharp.Html.Parser;
using Test.Properties;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //const string filePath = @"D:\迅雷下载\1\月神\[后藤佑迅][月神来我家! ][06].epub";

            //var epub = EpubBook.ReadEpub(File.ReadAllBytes(filePath),
            //    new FileStream(filePath, FileMode.Create), true);
            //epub.Dispose();

            MemoryStream stream = new();
            var epub = new EpubBook(stream);
            epub.AddCoverImage(new EpubItem { Data = Array.Empty<byte>(), ID = "Cover", EntryName = "Images/cover.jpg" });

            epub.Dispose();
            var data = stream.ToArray();
        }
    }
}