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

    
}
