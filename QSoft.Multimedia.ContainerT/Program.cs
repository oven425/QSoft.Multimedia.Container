// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


//using (var stream_w = File.Open("aa.mkv", FileMode.OpenOrCreate))
//{
//    QSoft.Multimedia.Container.MkvWriter mkvw = new QSoft.Multimedia.Container.MkvWriter(stream_w);
//    mkvw.Open();
//}



//using var stream = File.OpenRead("aa.mkv");
using var stream = File.OpenRead("../../../../matroska_test/testt.mkv");
QSoft.Multimedia.Container.MkvReader mkvr = new QSoft.Multimedia.Container.MkvReader(stream);
mkvr.Open();
foreach(var oo in mkvr)
{

}