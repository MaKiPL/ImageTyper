using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTyper
{
    internal static class MathExtended
    {
        public static int Clamp(int value, int maxValue)
        {
            if (value > maxValue)
                return maxValue;
            else return value;
        }
        public static int ClampUnsigned(int value, int maxValue)
        {
            if (value <= 0)
                return 0;
            if (value > maxValue)
                return maxValue;
            else return value;
        }

        public static byte MostRepetativeByte(byte[] buffer, int pass = 1)
        {
            uint[] candidates = new uint[256];
            for (int i = 0; i != buffer.Length; i += pass)
            {
                for (int ii = 0; ii != 255; ii++)
                    if (buffer[i] == ii)
                        candidates[ii]++;
            }
            int e = 0;
            for(int i = 1; i!=255; i++)            
                if (candidates[i] > candidates[i - 1])
                    e = i;
            //return e as byte?;
            return (byte)e;
        }
    }
}
