using QSoft.Media.Container.MP4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Mp4Reader mp4r = new Mp4Reader();
            mp4r.Open("../../../sample-mp4-file-small (2).mp4");
        }
    }
}
