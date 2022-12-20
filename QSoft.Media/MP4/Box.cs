using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Media.MP4
{
    public class box
    {
        public int Size { set; get; }
        public string Type { set; get; }
        public long BigSize { set; get; }
    }

    public class ftyp: box
    {
        public string major_brand { set; get; }
        public int minor_version { set; get; }
        public string[] compatible_brands { set; get; }
    }

    public class moox: box
    {

    }
}
