using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace BFRES
{
    public class FileOutput
    {
        List<byte> data = new List<byte>();

        public Endianness Endian = Endianness.Big;

        public byte[] getBytes()
        {
            return data.ToArray();
        }

        public void writeString(String s)
        {
            char[] c = s.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public void writeStringLen(String s)
        {
            char[] c = s.ToCharArray();
            writeInt(c.Length);
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
            data.Add(0x00);
            align(4);
        }

        public int size()
        {
            return data.Count;
        }

        public void writeOutput(FileOutput d)
        {
            foreach (int key in offsetCalculation.Keys)
            {
                if (offsetCalculation[key] == d)
                    writeIntAt((peekIntAt(key) + size()) - key, key);
            }

            foreach (int key in d.offsetCalculation.Keys)
                offsetCalculation.Add(key, d.offsetCalculation[key]);

            foreach (byte b in d.data)
                data.Add(b);
        }

        public int getStringOffset(string s)
        {
            char[] c = s.ToCharArray();
            for(int i =0; i < data.Count; i++)
            {
                int j = 0;
                for (j = 0;j < s.Length; j++)
                {
                    if (data[i + j] != c[j])
                        break;
                }
                if (j == s.Length)
                    return i;
            }
            int p = size();
            writeStringLen(s);
            return p+4;
        }

        public int peekIntAt(int p)
        {
            if (Endian == Endianness.Little)
            {
                return (data[p++] & 0xFF) | ((data[p++] & 0xFF) << 8) | ((data[p++] & 0xFF) << 16) | ((data[p++] & 0xFF) << 24);
            }
            else
                return ((data[p++] & 0xFF) << 24) | ((data[p++] & 0xFF) << 16) | ((data[p++] & 0xFF) << 8) | (data[p++] & 0xFF);
        }

        private static char[] HexToCharArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .Select(x => Convert.ToChar(x))
                             .ToArray();
        }

        public void writeHex(string s)
        {
            char[] c = HexToCharArray(s);
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public void writeInt(int i)
        {
            if (Endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 24) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 24) & 0xFF));
                data.Add((byte)((i >> 16) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }
        
        public void writeIntAt(int i, int p)
        {
            if (Endian == Endianness.Little)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 24) & 0xFF);
            }
            else
            {
                data[p++] = (byte)((i >> 24) & 0xFF);
                data[p++] = (byte)((i >> 16) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }
        public void writeShortAt(int i, int p)
        {
            if (Endian == Endianness.Little)
            {
                data[p++] = (byte)((i) & 0xFF);
                data[p++] = (byte)((i >> 8) & 0xFF);
            }
            else
            {
                data[p++] = (byte)((i >> 8) & 0xFF);
                data[p++] = (byte)((i) & 0xFF);
            }
        }

        public void align(int i)
        {
            while (data.Count % i != 0)
                writeByte(0);
        }

        public void align(int i, int v)
        {
            while (data.Count % i != 0)
                writeByte(v);
        }

        /*public void align(int i, int value){
			while(data.size() % i != 0)
				writeByte(value);
		}*/


        Dictionary<int, FileOutput> offsetCalculation = new Dictionary<int, FileOutput>();
        public void writeOffset(int i, FileOutput o)
        {
            offsetCalculation.Add(data.Count, o);
            writeInt(i);
        }


        public void writeFloat(float f)
        {
            int i = SingleToInt32Bits(f, Endian == Endianness.Big);
            data.Add((byte)((i) & 0xFF));
            data.Add((byte)((i >> 8) & 0xFF));
            data.Add((byte)((i >> 16) & 0xFF));
            data.Add((byte)((i >> 24) & 0xFF));
        }
     


        public static int SingleToInt32Bits(float value, bool littleEndian)
        {
            byte[] b = BitConverter.GetBytes(value);
            int p = 0;

            if (!littleEndian)
            {
                return (b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 24);
            }
            else
                return ((b[p++] & 0xFF) << 24) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 8) | (b[p++] & 0xFF);
        }

        public void writeHalfFloat(float f)
        {
            int i = fromFloat(f, Endian == Endianness.Little);
            data.Add((byte)((i >> 8) & 0xFF));
            data.Add((byte)((i) & 0xFF));
        }

        public static int fromFloat(float fval, bool littleEndian)
        {
            int fbits = FileOutput.SingleToInt32Bits(fval, littleEndian);
            int sign = fbits >> 16 & 0x8000;          // sign only
            int val = (fbits & 0x7fffffff) + 0x1000; // rounded value

            if (val >= 0x47800000)               // might be or become NaN/Inf
            {                                     // avoid Inf due to rounding
                if ((fbits & 0x7fffffff) >= 0x47800000)
                {                                 // is or must become NaN/Inf
                    if (val < 0x7f800000)        // was value but too large
                        return sign | 0x7c00;     // make it +/-Inf
                    return sign | 0x7c00 |        // remains +/-Inf or NaN
                        (fbits & 0x007fffff) >> 13; // keep NaN (and Inf) bits
                }
                return sign | 0x7bff;             // unrounded not quite Inf
            }
            if (val >= 0x38800000)               // remains normalized value
                return sign | val - 0x38000000 >> 13; // exp - 127 + 15
            if (val < 0x33000000)                // too small for subnormal
                return sign;                      // becomes +/-0
            val = (fbits & 0x7fffffff) >> 23;  // tmp exp for subnormal calc
            return sign | ((fbits & 0x7fffff | 0x800000) // add subnormal bit
                + (0x800000 >> val - 102)     // round depending on cut off
                >> 126 - val);   // div by 2^(1-(exp-127+15)) and >> 13 | exp=0
        }

        public void writeShort(int i)
        {
            if (Endian == Endianness.Little)
            {
                data.Add((byte)((i) & 0xFF));
                data.Add((byte)((i >> 8) & 0xFF));
            }
            else
            {
                data.Add((byte)((i >> 8) & 0xFF));
                data.Add((byte)((i) & 0xFF));
            }
        }

        public void writeByte(int i)
        {
            data.Add((byte)((i) & 0xFF));
        }

        public void writeChars(char[] c)
        {
            foreach (char ch in c)
                writeByte(Convert.ToByte(ch));
        }

        public void writeBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
                writeByte(b);
        }

        public void writeFlag(bool b)
        {
            if (b)
                writeByte(1);
            else
                writeByte(0);
        }

        public int pos()
        {
            return data.Count;
        }

        public void save(String fname)
        {
            File.WriteAllBytes(fname, data.ToArray());
        }
    }
}
