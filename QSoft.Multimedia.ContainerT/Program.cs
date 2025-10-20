// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


using (var stream_w = File.Open("../../../../matroska_test/aa.mkv", FileMode.OpenOrCreate))
{
    QSoft.Multimedia.Container.Mkv.MkvWriter mkvw = new(stream_w);
    mkvw.Open();
    //mkvw.WriteSegmentInfo(new QSoft.Multimedia.Container.Mkv.SegmentInfo()
    //{

    //});
}



//using var stream = File.OpenRead("aa.mkv");
//using var stream = File.OpenRead("../../../../matroska_test/testt.mkv");
using var stream = File.OpenRead("../../../../matroska_test/testt.mkv");

QSoft.Multimedia.Container.Mkv.MkvReader mkvr = new(stream);
mkvr.Open();


foreach(var oo in mkvr.GetAllFrames().Index())
{
    //File.WriteAllBytes($"{oo.Index}.h264", oo.Item.Raws.ElementAt(0));
}