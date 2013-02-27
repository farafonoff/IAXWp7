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
using Microsoft.Xna.Framework.Audio;

namespace Softphone
{
    public class AudioIn
    {
        private Microphone mic;
        int _msFrame;
        public AudioIn(int msFrame)
        {
            mic = Microphone.Default;
            mic.BufferDuration = TimeSpan.FromMilliseconds(400);
            mic.BufferReady += mic_BufferReady;
            _msFrame = msFrame;
        }
        byte[] intbuffer = null;

        void mic_BufferReady(object sender, EventArgs e)
        {
            Microphone mc = (Microphone)sender;
            //byte[] buffer = 
            /*byte[] buffer = new byte[mic.GetSampleSizeInBytes(mic.BufferDuration)];
            int bytesRead;
            while ((bytesRead = mic.GetData(buffer, 0, buffer.Length)) > 0)
            {
                int realcount = buffer.Length;
                for (int i = buffer.Length - 1; i > 0; --i)
                {
                    if (buffer[i] != 0)
                    {
                        realcount = i + 1;
                        if (realcount % 2 == 1) realcount += 1;
                        break;
                    }
                }
                procBuffer(buffer, realcount);
            }*/
            int bflen = mic.SampleRate / 1000 * _msFrame * 2; byte[] bf = new byte[bflen];
            int bytesRead;
            SampleEventArgs sa = new SampleEventArgs();
            sa.buffer = bf;
            sa.bytes = bflen;
            while ((bytesRead = mic.GetData(bf, 0, bf.Length)) > 0)
            {
                if (FrameReady!=null)
                    FrameReady(this, sa);
            }
        }
        void procBuffer(byte[] buffer, int realcount)
        {
            if (intbuffer != null)
            {
                byte[] nbuffer = new byte[intbuffer.Length + buffer.Length];
                Array.Copy(intbuffer, 0, nbuffer, 0, intbuffer.Length);
                Array.Copy(buffer, 0, nbuffer, intbuffer.Length, buffer.Length);
                intbuffer = null;
                buffer = nbuffer;
            }
            if (FrameReady != null)
            {
                SampleEventArgs sa = new SampleEventArgs();
                int bflen = mic.SampleRate / 1000 * _msFrame * 2;
                sa.src = mic;
                int i = 0;
                while (i < realcount)
                {
                    byte[] bf = new byte[bflen];
                    sa.buffer = bf;
                    sa.bytes = bflen;
                    Array.Copy(buffer, i, bf, 0, bflen);
                    FrameReady(this, sa);
                    i += bflen;
                }
                i -= bflen;
                if (realcount - i < bflen)
                {
                    intbuffer = new byte[realcount - i];
                    Array.Copy(buffer, i, intbuffer, 0, intbuffer.Length);
                }
            }
        }
        public delegate void SampleEventHandler(object sender, SampleEventArgs e);
        public event SampleEventHandler FrameReady;

        internal void Start()
        {
            mic.Start();
        }
    }
}
