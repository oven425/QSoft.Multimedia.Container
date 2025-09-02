using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Multimedia.Container
{
    public class MkvReader(Stream stream)
    {
        public void Open()
        {
            //Span<byte> buf = stackalloc byte[1024];
            //stream.Read(buf);
            //foreach(var oo in buf[0 .. 50])
            //{
            //    System.Diagnostics.Trace.Write($"{oo:X}-");
            //}
            var ebml_id = GetEBML_ID();

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
            if(length >1)
            {
                Span<byte> buffer = stackalloc byte[length-1];
                stream.Read(buffer);
                for (int i = 1; i < length; i++)
                    value = (value << 8) | buffer[i];
            }


            Span<byte> buffer1 = stackalloc byte[value*2];
            stream.Read(buffer1);
        }

        int GetEBML_ID()
        {
            var bb = stream.ReadByte();
            int id_len = bb switch
            {
                >= 0x80 => 1, // 1xxx xxxx
                >= 0x40 => 2, // 01xx xxxx
                >= 0x20 => 3, // 001x xxxx
                >= 0x10 => 4, // 0001 xxxx
                _ => throw new InvalidDataException("Invalid EBML ID")
            };
            Span<byte> buf = stackalloc byte[id_len - 1];
            stream.Read(buf);
            int id = bb;
            for (int i = 0; i < id_len - 1; i++)
            {
                id = (id << 8) | buf[i];
            }

            return id;
        }

        public void Close()
        {

        }
    }

}
