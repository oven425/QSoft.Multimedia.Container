// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var stream = File.OpenRead("../../../../matroska_test/test2.mkv");
QSoft.Multimedia.Container.MkvReader mkvr = new QSoft.Multimedia.Container.MkvReader(stream);
mkvr.Open();