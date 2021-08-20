using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Wuyu.Epub
{
    public static class Util
    {
        /// <summary>
        /// 相对路径转绝对路径
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 绝对路径转相对路径
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ZipRelativePath(string basePath,string path)
        {
            if (string.IsNullOrEmpty(basePath)) return path;
            basePath = Path.GetRelativePath(basePath, path).Replace(Path.DirectorySeparatorChar, '/');
            return basePath;
        }
    }
}
