using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Multimedia.Container
{
    public class MkvWriter(System.IO.Stream stream)
    {
        public void Open()
        {
            WritetEBML_ID(0x1a45dfa3);
            WriteEBML_Size(25);
        }

        void WritetEBML_ID(int data)
        {
            var bbs = BitConverter.GetBytes(data);
            Array.Reverse(bbs);
            var bb = bbs[0];
            int id_len = bb switch
            {
                >= 0x80 => 1, // 1xxx xxxx
                >= 0x40 => 2, // 01xx xxxx
                >= 0x20 => 3, // 001x xxxx
                >= 0x10 => 4, // 0001 xxxx
                _ => throw new InvalidDataException("Invalid EBML ID")
            };
            var buf = new Span<byte>(bbs)[..id_len];
            stream.Write(buf);

        }

        void WriteEBML_Size(int data)
        {
            var bitc = data switch
            {
                <= 127 => 0x80,
                <= 16383 => 0x40,
                <= 2097151 => 0x20,
                <= 268435455 => 0x10,
                _ => 0
            };


            var aaa = bitc | data;
            var bbs = BitConverter.GetBytes(aaa);
            Array.Reverse(bbs);
            stream.Write(bbs,0, bbs.Length);

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
