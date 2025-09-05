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
                        break;
                    case 0x114D9B74://SeekHead
                        break;
                    case 0x00004dbb://Seek
                        break;
                    case 0x000053ab://SeekID
                        stream.Position += ebml_size;
                        break;
                    case 0x000053ac://SeekPosition
                        stream.Position += ebml_size;
                        break;
                    case 0x1549A966://Segment info
                        break;
                    case 0x4489://Segment Duration
                        System.Diagnostics.Trace.WriteLine($"Duration:{ReadDouble(ebml_size)}");
                        break;
                    case 0x2AD7B1://TimestampScale
                        stream.Position += ebml_size;
                        break;
                    case 0x00004d80://MuxingApp
                        System.Diagnostics.Trace.WriteLine($"MuxingApp:{ReadString(ebml_size)}");
                        break;
                    case 0x5741://WritingApp
                        System.Diagnostics.Trace.WriteLine($"WritingApp:{ReadString(ebml_size)}");
                        break;
                    case 0x4461://DateUTC
                        System.Diagnostics.Trace.WriteLine($"DateUTC:{ReadUint(ebml_size)}");
                        break;
                    case 0x73A4://SegmentUUID
                        stream.Position += ebml_size;
                        break;
                    case 0x1654AE6B://Tracks
                        break;
                    case 0xAE://TrackEntry
                        break;
                    case 0xD7://TrackNumber
                        System.Diagnostics.Trace.WriteLine($"TrackNumber:{ReadUint(ebml_size)}");
                        break;
                    case 0x83://TrackType
                        System.Diagnostics.Trace.WriteLine($"TrackType:{ReadUint(ebml_size)}");
                        break;
                    case 0x73C5://TrackUID
                        System.Diagnostics.Trace.WriteLine($"TrackUID:{ReadUint(ebml_size)}");
                        break;
                    case 0x86://CodecID
                        System.Diagnostics.Trace.WriteLine($"CodecID:{ReadString(ebml_size)}");
                        break;
                    case 0x63A2://CodecPrivate
                        System.Diagnostics.Trace.WriteLine($"CodecPrivate:{BitConverter.ToString(ReadBlob(ebml_size))}");
                        break;
                    case 0x9C://FlagLacing
                        System.Diagnostics.Trace.WriteLine($"FlagLacing:{ReadUint(ebml_size)}");
                        break;
                    case 0x6DE7://MinCache
                        System.Diagnostics.Trace.WriteLine($"MinCache:{ReadUint(ebml_size)}");
                        break;
                    case 0x23E383://DefaultDuration
                        System.Diagnostics.Trace.WriteLine($"DefaultDuration:{ReadUint(ebml_size)}");
                        break;
                    case 0x22B59C://Language
                        System.Diagnostics.Trace.WriteLine($"Language:{ReadString(ebml_size)}");
                        break;
                    case 0xE0://Video
                        break;
                    case 0xB0://PixelWidth
                        System.Diagnostics.Trace.WriteLine($"PixelWidth:{ReadUint(ebml_size)}");
                        break;
                    case 0xBA://PixelHeight
                        System.Diagnostics.Trace.WriteLine($"PixelHeight:{ReadUint(ebml_size)}");
                        break;
                    case 0x54B0://DisplayWidth
                        System.Diagnostics.Trace.WriteLine($"DisplayWidth:{ReadUint(ebml_size)}");
                        break;
                    case 0xE1://Audio
                        break;
                    case 0xB5://SamplingFrequency
                        System.Diagnostics.Trace.WriteLine($"SamplingFrequency:{ReadDouble(ebml_size)}");
                        break;
                    case 0x9F://Channels
                        System.Diagnostics.Trace.WriteLine($"Channels:{ReadUint(ebml_size)}");
                        break;
                    case 0x1254C367://Tags
                        break;
                    case 0x7373://Tag
                        break;
                    case 0x63C0://Targets
                        break;
                    case 0x67C8://SimpleTag
                        break;
                    case 0x45A3://TagName
                        System.Diagnostics.Trace.WriteLine($"TagName:{ReadString(ebml_size)}");
                        break;
                    case 0x4487://TagString
                        System.Diagnostics.Trace.WriteLine($"TagString:{ReadString(ebml_size)}");
                        break;
                    case 0x1C53BB6B://Cues
                        break;
                    case 0xBB://CuePoint
                        break;
                    case 0xB3://CueTime
                        System.Diagnostics.Trace.WriteLine($"CueTime:{ReadUint(ebml_size)}");
                        break;
                    case 0xB7://CueTrackPositions
                        break;
                    case 0xBF://void
                    case 0x000000ec://void
                        stream.Position += ebml_size;
                        break;
                    default:
                        System.Diagnostics.Trace.WriteLine($"{ebml_id:X}");
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

        double ReadDouble(int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            stream.Read(buffer);

            return size switch
            {
                4 => BitConverter.Int32BitsToSingle(
                         BinaryPrimitives.ReadInt32BigEndian(buffer)),
                8 => BitConverter.Int64BitsToDouble(
                         BinaryPrimitives.ReadInt64BigEndian(buffer)),
                _ => throw new NotSupportedException()
            };
        }

        byte[] ReadBlob(int size)
        {
            var buf = new byte[size];
            stream.Read(buf, 0, buf.Length);
            return buf;
        }

        string ReadString(int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            stream.Read(buffer);
            return Encoding.UTF8.GetString(buffer);
        }

        uint ReadUint(int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            stream.Read(buffer);

            uint value = 0;
            for (int i=0;i<buffer.Length; i++)
            {
                value = (value << 8) | buffer[i];
            }
            return value;
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
}
