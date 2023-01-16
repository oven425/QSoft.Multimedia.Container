using System;
using System.Collections.Generic;
using System.IO;
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
        virtual public void Parse(BinaryReader br)
        {

        }
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
        public override void Parse(BinaryReader br)
        {
            base.Parse(br);
        }
    }

    public class moov: box
    {

    }

    public struct jj
    {
        public void hh()
        {

        }
    }

    public struct kkkk : jj
    {

    }

    public class fullbox : box
    {
        public byte version { set; get; }
        public byte[] flags { set; get; } = new byte[3];
        public override void Parse(BinaryReader br)
        {
            base.Parse(br);
        }
    }

    public class mvhd : fullbox
    {

    }

    public static class Mp4ParseEx
    {
        public static void Readmoov(this moov src, BinaryReader stream)
        {

        }
        public static int ReadInt(this BinaryReader src)
        {
            return BitConverter.ToInt32(src.ReadBytes(4).Reverse(), 0);
        }

        public static string ReadType(this BinaryReader src)
        {
            return Encoding.ASCII.GetString(src.ReadBytes(4));
        }
        public static ftyp Parsefype(this BinaryReader src, box box)
        {
            ftyp result = new ftyp(box);
            //result.Size = BitConverter.ToInt32(src.ReadBytes(4).Reverse(), 0);
            //result.Type = Encoding.ASCII.GetString(src.ReadBytes(4));
            result.major_brand = Encoding.ASCII.GetString(src.ReadBytes(4));
            result.minor_version = BitConverter.ToInt32(src.ReadBytes(4).Reverse(), 0);
            var com = Encoding.ASCII.GetString(src.ReadBytes(result.Size - (int)src.BaseStream.Position));
            return result;
        }

        public static moov Parsemoov(this BinaryReader src)
        {
            moov result = new moov();

            return result;
        }

        public static mvhd Parsemvhd(this BinaryReader src)
        {
            return new mvhd();
        }

        public static List<box> ParseBoxs(this BinaryReader src, box root)
        {
            var result = new List<box>();
            var endpos = src.BaseStream.Length;
            if (root != null)
            {
                src.BaseStream.Seek(root.Pos + root.Offset, SeekOrigin.Begin);
            }
            while (src.BaseStream.Position < endpos)
            {
                int offset = 8;
                var box = new box();
                box.Pos = src.BaseStream.Position;
                box.Size = src.ReadInt();
                box.Type = src.ReadType();
                if (box.Size == 1)
                {
                    box.BigSize = BitConverter.ToInt64(src.ReadBytes(8).Reverse(), 0);
                    offset = 16;
                }
                box.Offset = offset;
                src.BaseStream.Seek(box.Size - offset, SeekOrigin.Current);
                result.Add(box);
            }

            return result;
        }

        static byte[] Reverse(this byte[] src)
        {
            Array.Reverse(src);
            return src;
        }

    }
}
