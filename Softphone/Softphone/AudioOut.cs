﻿using System;
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
        public AudioOut()
        {
            //se = new SoundEffect(
            dse = new DynamicSoundEffectInstance(16000, AudioChannels.Mono);
            dse.BufferNeeded += new EventHandler<EventArgs>(dse_BufferNeeded);
            //_frag = fragmentMS;
        }
        int bufCount = 0;
        void dse_BufferNeeded(object sender, EventArgs e)
        {
            bufCount = 0;
            dse.Stop();
        }
        public void Start()
        {
            //jb = new JitterBuffer();
            dse.Play();
        }

        public void Stop()
        {
            //jb = null;
            dse.Stop();
        }

        public void playFragment(byte[] buffer)
        {
            ++bufCount;
            dse.SubmitBuffer(buffer);
            if (dse.PendingBufferCount >= 10 && dse.State == SoundState.Stopped && bufCount > 4)
            {
                dse.Play();
            }
        }
    }
}
