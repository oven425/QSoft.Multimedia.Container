using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//https://www.matroska.org/index.html
//https://blog.csdn.net/xuweilmy/article/details/8985002
namespace QSoft.Multimedia.Container
{
    public class MkvReader(Stream stream)
    {
        public void Open()
        {
            while (stream.Position < stream.Length)
            {
                var ebml_id = GetEBML_ID();
                var ebml_size = GetEBML_Size();
                switch (ebml_id)
                {
                    case 0x1a45dfa3:
                        ReadEBML(ebml_size);
                        break;
                    case 0x18538067://Segment
                        ReadSegment(ebml_size);
                        break;
                    case 0x114D9B74://SeekHead
                        ReadSeekHeader(ebml_size);
                        break;
                    case 0x00004dbb://Seek
                        break;
                    case 0x000053ab://SeekID
                        stream.Position += ebml_size;
                        break;
                    case 0x000053ac://SeekPosition
                        stream.Position += ebml_size;
                        break;
                    case 0xBF://void
                        stream.Position += ebml_size;
                        break;
                    default:
                        stream.Position += ebml_size;
                        break;
                }
            }
        }

        void ReadSeekHeader(int size)
        {

        }

        void ReadSegment(int size)
        {

        }

        void ReadEBML(int size)
        {
            stream.Position += size;

        }

        void EnumableEBML()
        {
            var ebml_id = GetEBML_ID();
            var ebml_size = GetEBML_Size();
            System.Diagnostics.Trace.WriteLine($"0x{ebml_id:X} {ebml_size}");
            EnumableEBML();
        }

        int GetEBML_Size()
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
                for (int i = 0; i < length-1; i++)
                    value = (value << 8) | buffer[i];
            }
            return value;
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

    public class EBMLHeader
    {
        
    }

}
