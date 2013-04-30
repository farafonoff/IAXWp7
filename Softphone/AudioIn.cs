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
    }
}
