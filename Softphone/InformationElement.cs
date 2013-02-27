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
using System.Text;

namespace Softphone
{
    public class InformationElement
    {
        public const byte CALLEDNUMBER = 0x01;
        public const byte MD5DIGEST = 0x02;

        public const byte USERNAME = 0x06;
        public const byte CAPABILITY = 0x08;
        public const byte FORMAT = 0x09;
        public const byte VERSION = 0x0b;
        public const byte AUTHMETHODS = 0x0e;
        public const byte CHALLENGE = 0x0f;
        public const byte MD5RESULT = 0x10;
        public const byte CALLINGPRES = 0x26;
        
        public byte type;
        public byte datalen;
        public byte[] data;
        public static UTF8Encoding utf8 = new UTF8Encoding();
        public InformationElement() { }
        public InformationElement(byte tp, string strdata)
        {
            type = tp;
            data = utf8.GetBytes(strdata);
            datalen = (byte)data.Length;
        }

        public InformationElement(byte p, byte[] data)
        {
            this.type = p;
            this.data = data;
            this.datalen = (byte)data.Length;
        }
    }
}
