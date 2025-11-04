using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSoft.Multimedia.Container.Mkv
{
    public static class EbmlExtension
    {
        public static int ReadEbmlID(this Stream stream)
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
        public static DateTime NanoSecToDateUTC(this ulong src)
        {
            var sec = src / 1000000000;
            var ts = TimeSpan.FromSeconds(sec);
            DateTime dd = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var d1 = dd + ts;
            return d1;
        }
    }
}
