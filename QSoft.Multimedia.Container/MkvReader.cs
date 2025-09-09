using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//https://www.matroska.org/index.html
//https://blog.csdn.net/xuweilmy/article/details/8985002
namespace QSoft.Multimedia.Container
{
    public class MkvReader(Stream stream)
    {
        //100000
        //475090
        //1000000000
        //47509000000
        //1000000
        public void Open()
        {
            var tts = TimeSpan.FromMilliseconds(47509);
            while (stream.Position < stream.Length)
            {
                var ebml_id = GetEBML_ID();
                var ebml_size = GetEBML_Size();
                switch (ebml_id)
                {
                    case 0x1a45dfa3:
                        m_Header = new EbmlHeader();
                        break;
                    case 0x4282://Uint DocTypes ID
                        if(m_Header  != null)
                            m_Header.DocTypes = ReadString(ebml_size);
                        break;
                    case 0x4287://Uint DocTypeVersion ID
                        if (m_Header != null)
                            m_Header.DocTypeVersion = ReadUint(ebml_size);
                        break;
                    case 0x4285://Uint DocTypeReadVersion ID
                        if (m_Header != null)
                            m_Header.DocTypeReadVersion = ReadUint(ebml_size);
                        break;
                    case 0x18538067://Segment
                        this.m_Segment = new Segment();
                        break;
                    case 0x114D9B74://SeekHead
                        if(this.m_Segment != null)
                            this.m_Segment.SeekHead = new SeekHead();
                        break;
                    case 0x00004dbb://Seek
                        if (this.m_Segment?.SeekHead != null)
                            this.m_Segment.SeekHead.Seeks.Add(new Seek());
                        break;
                    case 0x000053ab://SeekID
                        if (this.m_Segment?.SeekHead?.Seeks.Count > 0)
                            this.m_Segment.SeekHead.Seeks[^1].ID = ReadBlob(ebml_size);
                        break;
                    case 0x000053ac://SeekPosition
                        if (this.m_Segment?.SeekHead?.Seeks.Count > 0)
                            this.m_Segment.SeekHead.Seeks[^1].Position = ReadUint(ebml_size);
                        break;
                    case 0x1549A966://Segment info
                        if(this.m_Segment!= null)
                            this.m_Segment.SegmentInfo = new SegmentInfo();
                        break;
                    case 0x4489://Segment Duration
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.SegmentDuration = ReadDouble(ebml_size);
                        break;
                    case 0x2AD7B1://TimestampScale
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.TimestampScale = ReadUint(ebml_size);
                        break;
                    case 0x00004d80://MuxingApp
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.MuxingApp = ReadString(ebml_size);
                        break;
                    case 0x5741://WritingApp
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.WritingApp = ReadString(ebml_size);
                        break;
                    case 0x4461://DateUTC
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.DateUTC = ReadUint(ebml_size);
                        break;
                    case 0x73A4://SegmentUUID
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.SegmentUUID = ReadBlob(ebml_size);
                        break;
                    case 0x1654AE6B://Tracks
                        if(this.m_Segment != null)
                            this.m_Segment.Tracks = [];
                        break;
                    case 0xAE://TrackEntry
                        if (this.m_Segment?.Tracks != null)
                            this.m_Segment.Tracks.Add(new TrackEntry());
                        break;
                    case 0xD7://TrackNumber
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].TrackNumber = ReadUint(ebml_size);
                        break;
                    case 0x83://TrackType
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].TrackType = ReadUint(ebml_size);
                        break;
                    case 0x73C5://TrackUID
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].TrackUID = ReadUint(ebml_size);
                        break;
                    case 0x88://FlagDefault
                        System.Diagnostics.Trace.WriteLine($"FlagDefault:{ReadUint(ebml_size)}");
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
                    case 0x6D80://ContentEncodings
                        break;
                    case 0x6240://ContentEncoding
                        break;
                    case 0x5034://ContentCompression
                        break;
                    case 0x4254://ContentCompAlgo
                        System.Diagnostics.Trace.WriteLine($"ContentCompAlgo:{ReadUint(ebml_size)}");
                        break;
                    case 0x4255://ContentCompSettings
                        System.Diagnostics.Trace.WriteLine($"ContentCompSettings:{BitConverter.ToString(ReadBlob(ebml_size))}");
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
                    case 0xF7://CueTrack
                        System.Diagnostics.Trace.WriteLine($"CueTrack:{ReadUint(ebml_size)}");
                        break;
                    case 0xF1://CueClusterPosition
                        System.Diagnostics.Trace.WriteLine($"CueClusterPosition:{ReadUint(ebml_size)}");
                        break;
                    case 0x1F43B675://Cluster
                        break;
                    case 0xE7://Timestamp
                        System.Diagnostics.Trace.WriteLine($"Timestamp:{ReadUint(ebml_size)}");
                        break;
                    case 0xA0://BlockGroup
                        break;
                    case 0xA7://Position
                        System.Diagnostics.Trace.WriteLine($"Position:{ReadUint(ebml_size)}");
                        break;
                    case 0xA3://SimpleBlock
                        System.Diagnostics.Trace.WriteLine($"SimpleBlock:{ebml_size}");
                        stream.Position += ebml_size;
                        break;
                    case 0xA1://Block
                        stream.Position += ebml_size;
                        break;
                    case 0x9B://BlockDuration
                        System.Diagnostics.Trace.WriteLine($"BlockDuration:{ReadUint(ebml_size)}");
                        break;
                    case 0xAB://PrevSize
                        System.Diagnostics.Trace.WriteLine($"PrevSize:{ReadUint(ebml_size)}");
                        break;
                    case 0xFB://ReferenceBlock
                        stream.Position += ebml_size;
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

        Segment? m_Segment;
        EbmlHeader? m_Header;

        public TimeSpan Duration
        {
            get
            {
                if(this.m_Segment?.SegmentInfo != null)
                {
                    var tt = this.m_Segment.SegmentInfo.SegmentDuration * m_Segment.SegmentInfo.TimestampScale;
                    double milliseconds = tt / 1_000_000.0;
                    var ts = TimeSpan.FromMilliseconds(milliseconds);
                    return ts;
                }
                return TimeSpan.Zero;
            }
        }
    }

    public class TrackEntry
    {
        public uint TrackNumber { set; get; }
        public uint TrackType { set; get; }
        public uint TrackUID { set; get; }
    }


    public class Seek
    {
        public byte[] ID { set; get; }
        public uint Position { set; get; }
    }


    

    public class SegmentInfo
    {
        public double SegmentDuration { set; get; }
        public uint TimestampScale { set; get; }
        public string WritingApp { set; get; } = string.Empty;
        public string MuxingApp { set; get; } = string.Empty;
        public uint DateUTC { set; get; }

        public byte[] SegmentUUID { set; get; }
    }

    public class SeekHead
    {
        public List<Seek> Seeks { set; get; } = [];
    }
    public class Segment
    {
        public SegmentInfo? SegmentInfo { set; get; }
        public SeekHead? SeekHead { set; get; }
        public List<TrackEntry> Tracks {  set; get; }


    }
    public class EbmlHeader
    {
        public string DocTypes { set; get; }
        public uint DocTypeVersion { set; get; }
        public uint DocTypeReadVersion { set; get; }
    }


    
}
