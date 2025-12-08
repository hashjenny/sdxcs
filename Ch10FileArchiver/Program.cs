using System.IO.Abstractions;
using Ch10FileArchiver;

var archiver = new FileArchiver(new FileSystem(), sourceDir: "./Code");
foreach (var pair in archiver.Manifest) Console.WriteLine($"{pair.FileName}-{pair.FileHash}");