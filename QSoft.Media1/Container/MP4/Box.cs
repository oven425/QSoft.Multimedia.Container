using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Media.Container.MP4
{
    public class box
    {
        public box () { }
        public box(box box)
        {
            this.Pos = box.Pos;
            this.Offset= box.Offset;
            this.Size = box.Size;
            this.Type = box.Type;
            this.BigSize= box.BigSize;
        }
        public long Offset { set; get; }
        public long Pos { set; get; }
        public int Size { set; get; }
        public string Type { set; get; }
        public long BigSize { set; get; }
    }

    public class ftyp: box
    {
        public ftyp(box box)
            : base(box) 
        {
            
        }
        public string major_brand { set; get; }
        public int minor_version { set; get; }
        public string[] compatible_brands { set; get; }
    }

    public class moov: box
    {

    }

    public class fullbox : box
    {
        public byte version { set; get; }
        public byte[] flags { set; get; } = new byte[3];
    }

    public class mvhd : fullbox
    {

    }
}
