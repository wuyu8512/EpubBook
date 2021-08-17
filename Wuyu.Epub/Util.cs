using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Wuyu.Epub
{
    public static class Util
    {
        public static string ZipResolvePath(string basePath, string path)
        {
            var first = true;
            foreach (var item in path.Split('/'))
            {
                if (first && string.IsNullOrEmpty(item)) return path;
                if (item == "..")
                {
                    var temp = basePath.Split('/');
                    if (temp.Length == 1) basePath = "";
                    else basePath = string.Join('/', temp[..-1]);
                }
                else
                {
                    if (!string.IsNullOrEmpty(basePath) && !basePath.EndsWith("/")) basePath += "/";
                    basePath = basePath + item;
                }
            }
            return basePath;
        }
    }
}
