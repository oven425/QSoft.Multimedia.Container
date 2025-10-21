using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Multimedia.Container.Mkv
{
    public class MkvWriter(System.IO.Stream stream)
    {
        MemoryStream MemoryStream = new();
        public void Open()
        {
            EbmlHeader header = new()
            {
                EBMLVersion = 1,
                EBMLReadVersion = 1,
                EBMLMaxIDLength = 4,
                EBMLMAXSizeLength = 8,
                DocTypes = "matroska",
                DocTypeVersion = 4, 
                DocTypeReadVersion = 2,
            };
            MemoryStream.SetLength(0);
            WritetEBML_ID(0x4286, MemoryStream);
            WriteEBML_Uint(header.EBMLVersion, MemoryStream);
            WritetEBML_ID(0x42F7, MemoryStream);
            WriteEBML_Uint(header.EBMLReadVersion, MemoryStream);
            WritetEBML_ID(0x42F2, MemoryStream);
            WriteEBML_Uint(header.EBMLMaxIDLength, MemoryStream);
            WritetEBML_ID(0x42F3, MemoryStream);
            WriteEBML_Uint(header.EBMLMAXSizeLength, MemoryStream);
            WritetEBML_ID(0x4282, MemoryStream);
            WriteEBML_String("matroska", MemoryStream);
            WritetEBML_ID(0x4287, MemoryStream);
            WriteEBML_Uint(header.DocTypeVersion, MemoryStream);
            WritetEBML_ID(0x4285, MemoryStream);
            WriteEBML_Uint(header.DocTypeReadVersion, MemoryStream);


            WritetEBML_ID(0x1a45dfa3, stream);
            WriteEBML_VINT((int)MemoryStream.Length, stream);
            MemoryStream.Position = 0;
            MemoryStream.CopyTo(stream);

        }



        public void WriteSegmentInfo(SegmentInfo data)
        {
            //WritetEBML_ID(0x1549A966);
            //WriteEBML_VINT(0);
            //WritetEBML_ID(0x00004d80);
            //WriteEBML_String("12345");
        }

        public void AddTrack(int tracknum, string codec)
        {

        }


        void WriteEBML_Uint(uint data, Stream stream)
        {
            var count = data switch
            {
                <= 0xFF => (1),
                <= 0xFFFF => (2),
                <= 0xFFFFFF => (3),
                <= 268435455 => (4),
                _ => (0)
            };
            WriteEBML_VINT(count, stream);
            var s1 = MemoryMarshal.CreateSpan(ref data, 1);
            var s2 = MemoryMarshal.AsBytes(s1);
            var buf = s2[..count];
            buf.Reverse();
            stream.Write(buf);
        }

        void WriteEBML_String(string data, Stream stream)
        {
            WriteEBML_VINT(data.Length, stream);
            var buf = Encoding.UTF8.GetBytes(data);
            stream.Write(buf);

            //Span<byte> buffer = stackalloc byte[size];
            //stream.Read(buffer);
            //return Encoding.UTF8.GetString(buffer);
        }

        void WritetEBML_ID(int data, Stream stream)
        {
            var count = data switch
            {
                >=0x00FFFFFF => 4,
                >=0x0000FFFF =>3,
                >=0x000000FF=>2,
                >=0x00000000=>1,
                _=> throw new InvalidDataException("Invalid EBML ID")
            };
            var s1 = MemoryMarshal.CreateSpan(ref data, 1);
            var s2 = MemoryMarshal.AsBytes(s1);
            var buf = s2[..count];
            buf.Reverse();
            stream.Write(buf);

            //var bbs = BitConverter.GetBytes(data);
            //Array.Reverse(bbs);
            //var bb = bbs[0];
            //int id_len = bb switch
            //{
            //    >= 0x80 => 1, // 1xxx xxxx
            //    >= 0x40 => 2, // 01xx xxxx
            //    >= 0x20 => 3, // 001x xxxx
            //    >= 0x10 => 4, // 0001 xxxx
            //    _ => throw new InvalidDataException("Invalid EBML ID")
            //};
            //var buf = new Span<byte>(bbs)[..id_len];
            //stream.Write(buf);

        }

        void WriteEBML_VINT(int data, Stream stream)
        {
            var (bitmask, count) = data switch
            {
                <= 127 => (0x80, 1),
                <= 16383 => (0x40,2),
                <= 2097151 => (0x20,3),
                <= 268435455 => (0x10,4),
                _ => (0,0)
            };
            var aaa = bitmask | data;
            var s1 = MemoryMarshal.CreateSpan(ref aaa, 1);
            var s2 = MemoryMarshal.AsBytes(s1);
            var buf = s2[..count];
            buf.Reverse();
            stream.Write(buf);

            //var aaa = bitc | data;
            //var bbs = BitConverter.GetBytes(aaa);
            //Array.Reverse(bbs);
            //stream.Write(bbs,0, bbs.Length);

        }


        //VINT 長度 有效位元數 最大值
        //1 byte	7 bits	127
        //2 bytes	14 bits	16,383
        //3 bytes	21 bits	2,097,151
        //4 bytes	28 bits	268,435,455
        //5 bytes	35 bits	34,359,738,367
        //6 bytes	42 bits	4,398,046,511,103
        //7 bytes	49 bits	562,949,953,421,311
        //8 bytes	56 bits	72,057,594,037,927,935

        //By Copilot
        int GetEbmlVintLength(ulong value)
        {
            for (int length = 1; length <= 8; length++)
            {
                ulong maxValue = (1UL << (7 * length)) - 1;
                if (value <= maxValue)
                    return length;
            }
            throw new ArgumentOutOfRangeException(nameof(value), "Value too large for EBML VINT");
        }
    }
}
