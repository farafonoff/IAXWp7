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
        private CaptureSource capSource;
        private MemoryAudioSink sink;
        int _msFrame;
        public AudioIn(int msFrame)
        {
            capSource = new CaptureSource();
            capSource.AudioCaptureDevice = CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();
            //mic.BufferDuration = TimeSpan.FromMilliseconds(400);
            //mic.BufferReady += mic_BufferReady;
            _msFrame = msFrame;
            capSource.AudioCaptureDevice.AudioFrameSize = msFrame;
        }

        public void Start()
        {
            App.RunOnUI(() =>
            {
                capSource.Stop();
                sink = new MemoryAudioSink();
                sink.CaptureSource = capSource;
                sink.OnBufferFulfill += new EventHandler<GenericEventArgs<byte[]>>(sink_OnBufferFulfill);
                capSource.Start();
            });
        }

        void sink_OnBufferFulfill(object sender, GenericEventArgs<byte[]> e)
        {
            //LogService.Log(e.data.Length.ToString());
            if (FrameReady != null)
            {
                //FrameReady.BeginInvoke(this, e, null, null);
                FrameReady(this, e);
            }
            //throw new NotImplementedException();
        }

        public delegate void SampleEventHandler(object sender, GenericEventArgs<byte[]> e);
        public event SampleEventHandler FrameReady;
        /*       byte[] intbuffer = null;

               void mic_BufferReady(object sender, EventArgs e)
               {
                   Microphone mc = (Microphone)sender;
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
         *         

               internal void Start()
               {
                   mic.Start();
               }*/
    }
}
