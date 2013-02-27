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
    public class AudioOut
    {
        DynamicSoundEffectInstance dse;
        int _frag;
        public AudioOut(int fragmentMS)
        {
            //se = new SoundEffect(
            dse = new DynamicSoundEffectInstance(16000, AudioChannels.Mono);
            dse.BufferNeeded += new EventHandler<EventArgs>(dse_BufferNeeded);
            _frag = fragmentMS;
        }

        void dse_BufferNeeded(object sender, EventArgs e)
        {
            if (jb == null)
            {
                dse.Stop();
            }
            byte[] buffer = new byte[dse.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(_frag))];
            jb.Read(buffer, 0, buffer.Length);
            dse.SubmitBuffer(buffer);
        }
        JitterBuffer jb = null;
        public void Start()
        {
            jb = new JitterBuffer();
            dse.Play();
        }

        public void Stop()
        {
            jb = null;
            dse.Stop();
        }

        public void playFragment(byte[] buffer)
        {
            if (jb == null) Start();
            if (jb != null)
            {
                jb.write(buffer, 0, buffer.Length);
            }
        }
    }
}
