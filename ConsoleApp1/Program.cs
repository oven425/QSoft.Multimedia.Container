using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QSoft.Media.Container.MP4;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Mp4Reader reader = new Mp4Reader();
            reader.Open("../../../sample-mp4-file-small (2).mp4");
        }
    }
}
