namespace Ozeki.Media.Codec
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class Converters
    {
        public static byte[] ToByteArray(this short[] array)
        {
            byte[] buffer = new byte[array.Length * 2];
            for (int i = 0; i < array.Length; i++)
            {
                int index = 2 * i;
                buffer[index] = (byte) (array[i] & 0xff);
                buffer[index + 1] = (byte) ((array[i] >> 8) & 0xff);
            }
            return buffer;
        }

        public static byte[] ToByteArray(this int[] array)
        {
            byte[] buffer = new byte[array.Length * 2];
            for (int i = 0; i < array.Length; i++)
            {
                int index = 2 * i;
                buffer[index] = (byte) (array[i] & 0xff);
                buffer[index + 1] = (byte) ((array[i] >> 8) & 0xff);
            }
            return buffer;
        }

        public static float[] ToFloatArray(this byte[] array)
        {
            float[] numArray = new float[array.Length / 2];
            for (int i = 0; (i + 2) <= array.Length; i += 2)
            {
                short num2 = (short) ((array[i + 1] << 8) | array[i]);
                float num3 = ((float) num2) / 32768f;
                numArray[i / 2] = num3;
            }
            return numArray;
        }

        public static short[] ToShortArray(this byte[] array)
        {
            short[] numArray = new short[array.Length / 2];
            int index = 0;
            int startIndex = 0;
            while (startIndex < array.Length)
            {
                numArray[index] = BitConverter.ToInt16(array, startIndex);
                startIndex += 2;
                index++;
            }
            return numArray;
        }
    }
}

