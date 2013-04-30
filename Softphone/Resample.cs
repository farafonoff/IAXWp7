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
    public class Resample
    {
        public static byte[] to16(byte[] data, int offset, int count)
        {
            byte[] res = new byte[count*2];
            for (int i = 0; i < count; i+=2)
            {
                res[i * 2] = data[offset + i];
                res[i * 2+1] = data[offset + i+1];
            }
            return res;
        }
        public static byte[] from16(byte[] data, int offset, int count)
        {
            byte[] res = new byte[count / 2];
            for (int i = 0; i < res.Length; i+=2)
            {
                res[i] = data[offset + i*2];
                res[i + 1] = data[offset + i*2 + 1];
            }
            return res;
        }
    }
}
