using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Multimedia.Container
{
    public class MkvWriter(System.IO.Stream stream)
    {
        public void Open()
        {
            WritetEBML_ID(0x1a45dfa3);
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
            byte first = (byte)stream.ReadByte();
            int length = 1;
            byte mask = 0x80;
            while ((first & mask) == 0 && mask != 0)
            {
                mask >>= 1;
                length++;
            }

            if (length > 8)
                throw new InvalidOperationException("Invalid EBML size encoding");
            int value = (int)(first & (mask - 1));
            if (length > 1)
            {
                Span<byte> buffer = stackalloc byte[length - 1];
                stream.Read(buffer);
                for (int i = 0; i < length - 1; i++)
                    value = (value << 8) | buffer[i];
            }
            //return value;
        }

    }
}
