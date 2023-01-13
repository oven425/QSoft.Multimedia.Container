using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace QSoft.Media.Container.MP4
{
    public class Mp4Reader
    {
        FileStream m_File;
        BinaryReader m_Reader;
        public bool Open(string filename)
        {
            var moov = new moov();
            box bb = new moov();
            
            this.m_File = File.OpenRead(filename);
            this.m_Reader = new BinaryReader(this.m_File);
            var boxs = this.m_Reader.ParseBoxs(null);
            foreach(var box in boxs)
            {
                this.m_Reader.ParseBoxs(box);
            }
            return true;
        }


        public TimeSpan Duration { protected set; get; }


        public void Close()
        {
            if(this.m_File != null)
            {
                this.m_File.Close();
                this.m_File.Dispose();
                this.m_File = null;
            }
        }
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
            if(root!= null)
            {
                src.BaseStream.Seek(root.Pos+root.Offset, SeekOrigin.Begin);
            }
            while(src.BaseStream.Position < endpos)
            {
                int offset = 8;
                var box = new box();
                box.Pos= src.BaseStream.Position;
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
