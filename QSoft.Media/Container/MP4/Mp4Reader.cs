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
            this.m_File = File.OpenRead(filename);
            this.m_Reader = new BinaryReader(this.m_File);
            var ftyp = this.m_Reader.Parsefype();
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

        public static ftyp Parsefype(this BinaryReader src)
        {
            ftyp result = new ftyp();
            result.Size = BitConverter.ToInt32(src.ReadBytes(4).Reverse(), 0);
            result.Type = Encoding.ASCII.GetString(src.ReadBytes(4));
            result.major_brand = Encoding.ASCII.GetString(src.ReadBytes(4));
            result.minor_version = BitConverter.ToInt32(src.ReadBytes(4).Reverse(), 0);
            var com = Encoding.ASCII.GetString(src.ReadBytes(result.Size - (int)src.BaseStream.Position));
            return result;
        }

        static byte[] Reverse(this byte[] src)
        {
            Array.Reverse(src);
            return src;
        }



    }
}
