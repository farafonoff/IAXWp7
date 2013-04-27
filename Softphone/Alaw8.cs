using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Softphone
{
    public class Alaw8
    {
        public static short[] alaw_exp_table = new short[]{
      -5504, -5248, -6016, -5760, -4480, -4224, -4992, -4736,
      -7552, -7296, -8064, -7808, -6528, -6272, -7040, -6784,
      -2752, -2624, -3008, -2880, -2240, -2112, -2496, -2368,
      -3776, -3648, -4032, -3904, -3264, -3136, -3520, -3392,
     -22016,-20992,-24064,-23040,-17920,-16896,-19968,-18944,
     -30208,-29184,-32256,-31232,-26112,-25088,-28160,-27136,
     -11008,-10496,-12032,-11520, -8960, -8448, -9984, -9472,
     -15104,-14592,-16128,-15616,-13056,-12544,-14080,-13568,
       -344,  -328,  -376,  -360,  -280,  -264,  -312,  -296,
       -472,  -456,  -504,  -488,  -408,  -392,  -440,  -424,
        -88,   -72,  -120,  -104,   -24,    -8,   -56,   -40,
       -216,  -200,  -248,  -232,  -152,  -136,  -184,  -168,
      -1376, -1312, -1504, -1440, -1120, -1056, -1248, -1184,
      -1888, -1824, -2016, -1952, -1632, -1568, -1760, -1696,
       -688,  -656,  -752,  -720,  -560,  -528,  -624,  -592,
       -944,  -912, -1008,  -976,  -816,  -784,  -880,  -848,
       5504,  5248,  6016,  5760,  4480,  4224,  4992,  4736,
       7552,  7296,  8064,  7808,  6528,  6272,  7040,  6784,
       2752,  2624,  3008,  2880,  2240,  2112,  2496,  2368,
       3776,  3648,  4032,  3904,  3264,  3136,  3520,  3392,
      22016, 20992, 24064, 23040, 17920, 16896, 19968, 18944,
      30208, 29184, 32256, 31232, 26112, 25088, 28160, 27136,
      11008, 10496, 12032, 11520,  8960,  8448,  9984,  9472,
      15104, 14592, 16128, 15616, 13056, 12544, 14080, 13568,
        344,   328,   376,   360,   280,   264,   312,   296,
        472,   456,   504,   488,   408,   392,   440,   424,
         88,    72,   120,   104,    24,     8,    56,    40,
        216,   200,   248,   232,   152,   136,   184,   168,
       1376,  1312,  1504,  1440,  1120,  1056,  1248,  1184,
       1888,  1824,  2016,  1952,  1632,  1568,  1760,  1696,
        688,   656,   752,   720,   560,   528,   624,   592,
        944,   912,  1008,   976,   816,   784,   880,   848};
        public static byte[] alawtolin16(byte[] din,int offset, int count)
        {
            byte[] dout = new byte[count * 2 * 2];//16bit 16khz
            for (int i = 0; i < count; ++i)
            {
                short v = alaw_exp_table[din[i+offset]];
                var bv = BitConverter.GetBytes(v);
                for (int word = 0; word < 2; ++word)
                {
                    dout[i * 4 + word * 2] = bv[0];
                    dout[i * 4 + word * 2 + 1] = bv[1];
                }
            }
            return dout;
        }
        public static byte[] lin16toalaw(byte[] din, int offset, int count)
        {
            byte[] result = new byte[count / 4];
            int end = count + offset;
            for (int i = offset; i < end; i += 4)
            {
                short sv = BitConverter.ToInt16(din, i);
                result[(i - offset) / 4] = encode(sv);
            }
            return result;
        }
        public const int BIAS = 0x84; //132, or 1000 0100
        public const int MAX = 32635; //32767 (max 15-bit integer) minus BIAS
        private static byte encode(int pcm)
        {
            //Get the sign bit. Shift it for later use 
            //without further modification
            int sign = (pcm & 0x8000) >> 8;
            //If the number is negative, 
            //make it positive (now it's a magnitude)
            if (sign != 0)
                pcm = -pcm;
            //The magnitude must fit in 15 bits to avoid overflow
            if (pcm > MAX) pcm = MAX;

            /* Finding the "exponent"
             * Bits:
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 7 6 5 4 3 2 1 0 0 0 0 0 0 0 0
             * We want to find where the first 1 after the sign bit is.
             * We take the corresponding value 
             * from the second row as the exponent value.
             * (i.e. if first 1 at position 7 -> exponent = 2)
             * The exponent is 0 if the 1 is not found in bits 2 through 8.
             * This means the exponent is 0 even if the "first 1" doesn't exist.
             */
            int exponent = 7;
            //Move to the right and decrement exponent 
            //until we hit the 1 or the exponent hits 0
            for (int expMask = 0x4000; (pcm & expMask) == 0
                 && exponent > 0; exponent--, expMask >>= 1) { }

            /* The last part - the "mantissa"
             * We need to take the four bits after the 1 we just found.
             * To get it, we shift 0x0f :
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 0 0 0 0 0 1 . . . . . . . . . (say that exponent is 2)
             * . . . . . . . . . . . . 1 1 1 1
             * We shift it 5 times for an exponent of two, meaning
             * we will shift our four bits (exponent + 3) bits.
             * For convenience, we will actually just
             * shift the number, then AND with 0x0f. 
             * 
             * NOTE: If the exponent is 0:
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 0 0 0 0 0 0 0 Z Y X W V U T S (we know nothing about bit 9)
             * . . . . . . . . . . . . 1 1 1 1
             * We want to get ZYXW, which means a shift of 4 instead of 3
             */
            int mantissa = (pcm >> ((exponent == 0) ? 4 : (exponent + 3))) & 0x0f;

            //The a-law byte bit arrangement is SEEEMMMM 
            //(Sign, Exponent, and Mantissa.)
            byte alaw = (byte)(sign | exponent << 4 | mantissa);

            //Last is to flip every other bit, and the sign bit (0xD5 = 1101 0101)
            return (byte)(alaw ^ 0xD5);
        }
    }
}
