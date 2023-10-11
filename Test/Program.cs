using System;
using System.Diagnostics;
using Wuyu.Epub;


var stopwatch = new Stopwatch();
stopwatch.Restart();
var epub = await EpubBook.ReadEpubAsync(@"F:\轻小说\Liberex - Test\library\魔女と傭兵.epub");

Console.WriteLine(stopwatch.ElapsedMilliseconds);

epub.Dispose();

Console.WriteLine(stopwatch.ElapsedMilliseconds);

Console.ReadLine();