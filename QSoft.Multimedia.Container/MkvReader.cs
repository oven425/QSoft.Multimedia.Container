using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
namespace QSoft.Multimedia.Container.Mkv
{
    public class MkvReader(Stream stream)
    {
        int m_SegmentOffset = 0;
        public void Open()
        {
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
                    case 0x4286://EBMLVersion
                        if (m_Header != null)
                            m_Header.EBMLVersion = ReadUint(ebml_size);
                        break;
                    case 0x42F7://EBMLReadVersion
                        if (m_Header != null)
                            m_Header.EBMLReadVersion = ReadUint(ebml_size);
                        break;
                    case 0x42F2://EBMLMaxIDLength
                        if (m_Header != null)
                            m_Header.EBMLMaxIDLength = ReadUint(ebml_size);
                        break;
                    case 0x42F3://EBMLMAXSizeLength
                        if (m_Header != null)
                            m_Header.EBMLMAXSizeLength = ReadUint(ebml_size);
                        break;
                    case 0x18538067://Segment
                        m_SegmentOffset = (int)stream.Position;
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
                    case 0x7BA9://Title
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.Title = ReadString(ebml_size);
                        break;
                    case 0x4461://DateUTC
                        if (this.m_Segment?.SegmentInfo != null)
                            this.m_Segment.SegmentInfo.DateUTC = ReadUint64(ebml_size).NanoSecToDateUTC();
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
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].CodecID = ReadString(ebml_size);
                        break;
                    case 0x536E://Name
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].Name = ReadString(ebml_size);
                        break;
                    case 0x63A2://CodecPrivate
                        if (this.m_Segment?.Tracks.Count > 0)
                        {
                            this.m_Segment.Tracks[^1].CodecPrivate = ReadBlob(ebml_size);
                            if (this.m_Segment.Tracks[^1].CodecID == "V_MPEG4/ISO/AVC")
                            {
                                this.m_AVCDecoderConfigurationRecord = new AVCDecoderConfigurationRecord(this.m_Segment.Tracks[^1].CodecPrivate);
                            }
                            else if (this.m_Segment.Tracks[^1].CodecID == "V_MS/VFW/FOURCC")
                            {
                                var bh = MemoryMarshal.Read<BITMAPINFOHEADER>(this.m_Segment.Tracks[^1].CodecPrivate);
                            }
                        }
                        break;
                    case 0x22B59D:
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].LanguageBCP47 = ReadString(ebml_size);
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
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].Language = ReadString(ebml_size);
                        break;
                    case 0xE0://Video
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].Video = new();
                        break;
                    case 0xB0://PixelWidth
                        if (this.m_Segment?.Tracks.Count > 0)
                            if(this.m_Segment.Tracks[^1].Video is TrackEntryVideo vd)
                                vd.PixelWidth = ReadUint(ebml_size);
                        break;
                    case 0xBA://PixelHeight
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Video is TrackEntryVideo vd)
                                vd.PixelHeight = ReadUint(ebml_size);
                        break;
                    case 0x54B0://DisplayWidth
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Video is TrackEntryVideo vd)
                                vd.DisplayWidth = ReadUint(ebml_size);
                        break;
                    case 0x54BA://DisplayHeight
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Video is TrackEntryVideo vd)
                                vd.DisplayHeight = ReadUint(ebml_size);
                        break;
                    case 0xE1://Audio
                        if (this.m_Segment?.Tracks.Count > 0)
                            this.m_Segment.Tracks[^1].Audio = new();
                        break;
                    case 0xB5://SamplingFrequency
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Audio is TrackEntryAudio ad)
                                ad.SamplingFrequency = ReadDouble(ebml_size);
                        break;
                    case 0x78B5:
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Audio is TrackEntryAudio ad)
                                ad.OutputSamplingFrequency = ReadDouble(ebml_size);
                        break;
                    case 0x9F://Channels
                        if (this.m_Segment?.Tracks.Count > 0)
                            if (this.m_Segment.Tracks[^1].Audio is TrackEntryAudio ad)
                                ad.Channels = ReadUint(ebml_size);
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
                        if(this.m_Segment != null)
                            this.m_Segment.Tags = [];
                        break;
                    case 0x7373://Tag
                        this.m_Segment?.Tags.Add(new Tag());
                        break;
                    case 0x63C0://Targets
                        if (this.m_Segment?.Tags?.Count > 0)
                            this.m_Segment.Tags[^1].SimpleTag.Add(new TargetTag());
                        break;
                    case 0x68CA://TargetTypeValue
                        if (this.m_Segment?.Tags.Count > 0 && this.m_Segment.Tags[^1].SimpleTag.Count > 0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is TargetTag st)
                                st.TargetTypeValue = ReadUint(ebml_size);
                        break;
                    case 0x67C8://SimpleTag
                        if (this.m_Segment?.Tags?.Count > 0)
                            this.m_Segment.Tags[^1].SimpleTag.Add(new SimpleTag());
                        break;
                    case 0x45A3://TagName
                        if(this.m_Segment?.Tags.Count>0 && this.m_Segment.Tags[^1].SimpleTag.Count>0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is SimpleTag st)
                                st.TagName = ReadString(ebml_size);
                        break;
                    case 0x4487://TagString
                        if (this.m_Segment?.Tags.Count > 0 && this.m_Segment.Tags[^1].SimpleTag.Count > 0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is SimpleTag st)
                                st.TagString = ReadString(ebml_size);
                        break;
                    case 0x447A://TagLanguage
                        if (this.m_Segment?.Tags.Count > 0 && this.m_Segment.Tags[^1].SimpleTag.Count > 0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is SimpleTag st)
                                st.TagLanguage = ReadString(ebml_size);
                        break;
                    case 0x4484://TagDefault
                        if (this.m_Segment?.Tags.Count > 0 && this.m_Segment.Tags[^1].SimpleTag.Count > 0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is SimpleTag st)
                                st.TagDefault = this.ReadUint(ebml_size);
                        break;
                    case 0x63C5://TagTrackUID
                        if (this.m_Segment?.Tags.Count > 0 && this.m_Segment.Tags[^1].SimpleTag.Count > 0)
                            if (this.m_Segment.Tags[^1].SimpleTag[^1] is TargetTag st)
                                st.TagTrackUID = this.ReadUint64(ebml_size);
                        break;
                    case 0x1C53BB6B://Cues
                        if(this.m_Segment != null)
                            this.m_Segment.Cues = [];
                        break;
                    case 0xBB://CuePoint
                        this.m_Segment?.Cues.Add(new CuePoint());
                        break;
                    case 0xB3://CueTime
                        if (this.m_Segment?.Cues.Count > 0)
                            this.m_Segment.Cues[^1].CueTime = ReadUint(ebml_size);
                        break;
                    case 0xB7://CueTrackPositions
                        if (this.m_Segment?.Cues.Count > 0)
                            this.m_Segment.Cues[^1].CueTrackPositions.Add(new CueTrackPosition());
                        break;
                    case 0xF7://CueTrack
                        if (this.m_Segment?.Cues.Count > 0 && this.m_Segment?.Cues[^1].CueTrackPositions.Count > 0)
                            this.m_Segment.Cues[^1].CueTrackPositions[^1].CueTrack = ReadUint(ebml_size);
                        break;
                    case 0xF1://CueClusterPosition
                        if (this.m_Segment?.Cues.Count > 0 && this.m_Segment?.Cues[^1].CueTrackPositions.Count > 0)
                            this.m_Segment.Cues[^1].CueTrackPositions[^1].CueClusterPosition = ReadUint(ebml_size);
                        break;
                    case 0xF0://CueRelativePosition
                        if (this.m_Segment?.Cues.Count > 0 && this.m_Segment?.Cues[^1].CueTrackPositions.Count > 0)
                            this.m_Segment.Cues[^1].CueTrackPositions[^1].CueRelativePosition = ReadUint(ebml_size);
                        break;
                    case 0x1F43B675://Cluster
                        if(this.m_Segment!=null)
                            this.m_Segment.Clusters.Add(new Cluster());
                        break;
                    case 0xE7://Timestamp
                        if(this.m_Segment?.Clusters.Count >0)
                            this.m_Segment.Clusters[^1].Timestamp = ReadUint(ebml_size);
                        break;
                    case 0xA0://BlockGroup
                        System.Diagnostics.Trace.WriteLine($"BlockGroup:{ebml_size}");
                        break;
                    case 0xA7://Position
                        if (this.m_Segment?.Clusters.Count > 0)
                            this.m_Segment.Clusters[^1].Position = ReadUint(ebml_size);
                        break;
                    case 0xA3://SimpleBlock
                        var sb = new SimpleBlock();
                        sb.TrackNum = GetEBML_Size();
                        byte[] timecode = new byte[2];
                        stream.Read(timecode, 0, timecode.Length);
                        Array.Reverse(timecode);
                        sb.TimeCode = BitConverter.ToUInt16(timecode, 0);
                        sb.Flag = (byte)stream.ReadByte();
                        if (this.m_Segment?.Clusters.Count > 0)
                            this.m_Segment.Clusters[^1].SimpleBlocks.Add(sb);
                        sb.RawPos = stream.Position;
                        sb.RawSize = ebml_size - 4;
                        stream.Position = stream.Position + sb.RawSize;

                        break;
                    case 0xA1://Block
                        System.Diagnostics.Trace.WriteLine($"Block:{ebml_size}");
                        stream.Position += ebml_size;
                        break;
                    case 0x9B://BlockDuration
                        System.Diagnostics.Trace.WriteLine($"BlockDuration:{ReadUint(ebml_size)}");
                        break;
                    case 0xAB://PrevSize
                        if (this.m_Segment?.Clusters.Count > 0)
                            this.m_Segment.Clusters[^1].PrevSize = ReadUint(ebml_size);
                        break;
                    case 0xFB://ReferenceBlock
                        System.Diagnostics.Trace.WriteLine($"ReferenceBlock:{ReadUint(ebml_size)}");
                        break;
                    case 0xBF://void
                    case 0x000000ec://void
                        stream.Position += ebml_size;
                        break;
                    default:
                        System.Diagnostics.Trace.WriteLine($"id:0x{ebml_id:X} size:{ebml_size}");
                        stream.Position += ebml_size;
                        break;
                }
            }
        }

        void ParseH264(long rawsz, List<byte[]> rawbuf, bool iskeyframe)
        {
            BinaryReader br = new(stream);
            var pos1 = br.BaseStream.Position;
            List<byte> raww = [];
            var sz = rawsz;
            if(iskeyframe && m_AVCDecoderConfigurationRecord is not null)
            {
                raww.AddRange([0x00, 0x00, 0x00, 0x01]);
                raww.AddRange(this.m_AVCDecoderConfigurationRecord.SPSs[0]);
                raww.AddRange([0x00, 0x00, 0x00, 0x01]);
                raww.AddRange(this.m_AVCDecoderConfigurationRecord.PPSs[0]);
            }
            while (true)
            {
                var aa = br.ReadBytes(4);
                Array.Reverse(aa);
                var raw_size = BitConverter.ToInt32(aa);
                var raw = new byte[raw_size];
                br.BaseStream.Read(raw);
                raww.AddRange([0x00, 0x00, 0x00, 0x01]);
                raww.AddRange(raw);
                var sz1 = br.BaseStream.Position - pos1;
                if (sz1 == sz)
                {
                    rawbuf.Add([.. raww]);
                    break;
                }
            }
        }
        public IEnumerable<FrameIndex> GetAllFrames()
        {
            if(this.m_Segment is null) yield break;
            List<byte[]> rawbuf = [];
            foreach (var oo in this.m_Segment.Clusters)
            {
                foreach (var ooo in oo.SimpleBlocks)
                {
                    rawbuf.Clear();
                    var index = new FrameIndex
                    {
                        Posisiotn = oo.Position,
                        Time = TimeSpan.FromMilliseconds(oo.Timestamp + ooo.TimeCode),
                        TrackNum = ooo.TrackNum,
                        IsKeyFrame = ooo.IsKeyFrame
                    };
                    stream.Position = ooo.RawPos;
                    var track = this.m_Segment.Tracks.FirstOrDefault(x => x.TrackNumber == index.TrackNum);
                    if(track?.TrackType == 1)
                    {
                        ParseH264(ooo.RawSize, rawbuf, ooo.IsKeyFrame);
                        index.Raws = [.. rawbuf];
                    }
                    else if(track?.TrackType == 2)
                    {
                        if(ooo.Lacing==3)
                        {
                            var count = stream.ReadByte();
                            var basicsize = this.GetEBML_Size();
                            List<byte[]> szs = [new byte[basicsize]];
                            for (int i=0; i<count-1; i++)
                            {
                                var sz1 = this.GetEBML_Int();
                                basicsize = basicsize + sz1;
                                szs.Add(new byte[basicsize]);
                            }
                            var sss = ooo.RawSize - szs.Sum(x=>x.Length) - (stream.Position-ooo.RawPos);
                            szs.Add(new byte[sss]);
                            
                            foreach (var sz in szs)
                            {
                                stream.Read(sz);
                            }
                            index.Raws = [.. szs];
                        }
                    }
                        
                    yield return index;
                }
            }
        }


        int GetEBML_Int()
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

            //2 ^ ((7 * n) - 1) ^ -1
            var aa = (int)Math.Pow(2, 7 * length - 1) - 1;
            value = value - aa;
            return value;
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

        ulong ReadUint64(int size)
        {
            Span<byte> buffer = stackalloc byte[size];
            stream.Read(buffer);

            ulong value = 0;
            for (int i = 0; i < buffer.Length; i++)
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


        AVCDecoderConfigurationRecord? m_AVCDecoderConfigurationRecord;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;              // DWORD = 4 bytes
        public int biWidth;              // LONG = 4 bytes
        public int biHeight;             // LONG = 4 bytes
        public ushort biPlanes;          // WORD = 2 bytes
        public ushort biBitCount;        // WORD = 2 bytes
        public uint biCompression;       // DWORD = 4 bytes
        public uint biSizeImage;         // DWORD = 4 bytes
        public int biXPelsPerMeter;      // LONG = 4 bytes
        public int biYPelsPerMeter;      // LONG = 4 bytes
        public uint biClrUsed;           // DWORD = 4 bytes
        public uint biClrImportant;      // DWORD = 4 bytes
    }

    public class SimpleBlock
    {
        public int TrackNum { set; get; }
        public ushort TimeCode { set; get; }
        public byte Flag { set; get; }
        public bool IsKeyFrame => (this.Flag & 0x80) >> 7 == 0x01;
        public bool CanDrop => (this.Flag & 0x01) == 0x01;
        /// <summary>
        /// 00:no lacing, 01:Xiph lacing, 10:fixed-size lacing, 11:EBML lacing
        /// </summary>
        public byte Lacing=> (byte)((this.Flag & 0x06) >> 1);
        public bool IsDispaly => (this.Flag & 0x07) >> 3 == 0x01;
        public long RawPos { set; get; }
        public long RawSize { set; get; }
    }

    public class Cluster
    {
        public uint Timestamp { set; get; }
        public uint Position { set; get; }
        public uint PrevSize { set; get; }

        public List<SimpleBlock> SimpleBlocks { set; get; } = [];
    }

    public class CueTrackPosition
    {
        public uint CueTrack { set; get; }
        public uint CueClusterPosition { set; get; }
        public uint CueRelativePosition { set; get; }
    }

    public class CuePoint
    {
        public uint CueTime { set; get; }
        public List<CueTrackPosition> CueTrackPositions { set; get; } = [];
    }

    public class TargetTag
    {
        public uint TargetTypeValue { set; get; }
        public ulong TagTrackUID { set; get; }
    }

    public class SimpleTag
    {
        public string TagName { get; set; } = string.Empty;
        public string TagString { get; set; } = string.Empty;
        public string TagLanguage { set; get; } = string.Empty;
        public uint? TagDefault { set; get; }
    }

    public class Tag
    {
        public List<object> SimpleTag { set; get; } = [];
    }

    public class TrackEntryVideo
    {
        public uint PixelWidth { set; get; }
        public uint PixelHeight { set; get; }
        public uint DisplayWidth { set; get; }
        public uint DisplayHeight { set; get; }
    }

    public class TrackEntryAudio
    {
        public double SamplingFrequency { set; get; }
        public double OutputSamplingFrequency { set; get; }
        public uint Channels { set; get; }
    }

    public class TrackEntry
    {
        public uint TrackNumber { set; get; }
        /// <summary>
        /// 1 - video, 2 - audio, 3 - complex, 16 - logo, 17 - subtitle, 18 - buttons, 32 - control, 33 - metadata
        /// </summary>
        public uint TrackType { set; get; }
        public uint TrackUID { set; get; }
        public string CodecID { set; get; } = string.Empty;
        public string Name { set; get; } =string.Empty;
        public byte[] CodecPrivate { set; get; } = [];
        public TrackEntryVideo? Video { set; get; }
        public TrackEntryAudio? Audio { set; get; }
        public string LanguageBCP47 { set; get; } = string.Empty;
        public string Language { set; get; } = string.Empty;



    }

    //[Codec Mappings](https://www.matroska.org/technical/codec_specs.html)
    //[音视频基础 FLV格式详解](https://zhuanlan.zhihu.com/p/406888863)
    public class AVCDecoderConfigurationRecord
    {
        public AVCDecoderConfigurationRecord(byte[] src)
        {
            int offset = 0;
            var span = src.AsSpan();
            ConfigurationVersion = src[0];
            AVCProfileIndication = src[1];
            profile_compatibility = src[2];
            AVCLevelIndication = src[3];
            lengthSizeMinusOne = (byte)(1+src[4]&0x03);
            numOfSequenceParameterSets = (byte)(src[5] & 0x1F);
            var s = span.Slice(6, 2);
            s.Reverse();
            sequenceParameterSetLength = BitConverter.ToInt16(s);
            var sps = span.Slice(8, sequenceParameterSetLength);
            SPSs.Add(sps.ToArray());
            numOfPictureParameterSets = (byte)(src[8+ sequenceParameterSetLength] & 0x1F);
            offset = 8 + sequenceParameterSetLength;
            s = span.Slice(offset + 1, 2);
            s.Reverse();
            pictureParameterSetLength = BitConverter.ToInt16(s);
            var pps = span.Slice(offset+3, pictureParameterSetLength);
            PPSs.Add(pps.ToArray());
        }

        public List<byte[]> SPSs { set; get; } = [];
        public List<byte[]> PPSs { set; get; } = [];
        public byte ConfigurationVersion { set; get; }
        public byte AVCProfileIndication { set; get; }
        public byte profile_compatibility { set; get; }
        public byte AVCLevelIndication { set; get; }

        public byte lengthSizeMinusOne { set; get; } //（NALUSize的长度，计算方法为：1 + (lengthSizeMinusOne & 3)=4）

        public byte numOfSequenceParameterSets { set; get; }//（低五位为SPS的个数，计算方法为：numOfSequenceParameterSets & 0x1F=1）
        
        public short sequenceParameterSetLength { set; get; }
        public byte numOfPictureParameterSets { set; get; }

        public short pictureParameterSetLength { set; get; }

    }

    

    public class Seek
    {
        public byte[]? ID { set; get; }
        public uint Position { set; get; }
    }


    

    public class SegmentInfo
    {
        public double SegmentDuration { set; get; }
        public uint TimestampScale { set; get; }
        public string WritingApp { set; get; } = string.Empty;
        public string MuxingApp { set; get; } = string.Empty;
        public DateTime DateUTC { set; get; }
        public string Title { set; get; } = string.Empty;
        public byte[] SegmentUUID { set; get; } = [];
    }

    public class SeekHead
    {
        public List<Seek> Seeks { set; get; } = [];
    }
    public class Segment
    {
        public SegmentInfo? SegmentInfo { set; get; }
        public SeekHead? SeekHead { set; get; }
        public List<TrackEntry> Tracks { set; get; } = [];

        public List<Tag> Tags { set; get; } = [];
        public List<CuePoint> Cues { set; get; } = [];
        public List<Cluster> Clusters { set; get; } = [];
    }
    public class EbmlHeader
    {
        public string DocTypes { set; get; } = string.Empty;
        public uint DocTypeVersion { set; get; }
        public uint DocTypeReadVersion { set; get; }
        public uint EBMLVersion { set; get; }
        public uint EBMLReadVersion { set; get; }
        public uint EBMLMaxIDLength { set; get; }
        public uint EBMLMAXSizeLength { set; get; }
    }

    public partial class FrameIndex
    {
        public int TrackNum { set; get; }
        public TimeSpan Time { set; get; }

        public long Posisiotn { set; get; }
        public bool IsKeyFrame { set; get; }
        public IEnumerable<byte[]> Raws { set; get; } = [];
    }
}
