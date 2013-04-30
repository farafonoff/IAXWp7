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
using System.Collections.Generic;
using System.Threading;

namespace Softphone
{
    public class AudioOut
    {
        DynamicSoundEffectInstance dse;

        System.Threading.Thread decodingThread = null;

        List<queueFragmentDelegate> fq = new List<queueFragmentDelegate>();
        
        public AudioOut()
        {
            //se = new SoundEffect(
            dse = new DynamicSoundEffectInstance(16000, AudioChannels.Mono);
            dse.BufferNeeded += new EventHandler<EventArgs>(dse_BufferNeeded);
            decodingThread = new System.Threading.Thread(decode_thread_fn);
            decodingThread.Start();
            //_frag = fragmentMS;
        }

        void decode_thread_fn()
        {
            while (true)
            {
                List<queueFragmentDelegate> eq = new List<queueFragmentDelegate>();
                lock (fq)
                {
                    if (fq.Count > 0)
                    {
                        eq.AddRange(fq);
                        fq.Clear();
                    } else
                    {
                        Monitor.Wait(fq);
                    }
                }
                foreach(queueFragmentDelegate de in eq)
                {
                    playFragment(de());
                }
                eq.Clear();
            }
        }
        int bufCount = 0;
        void dse_BufferNeeded(object sender, EventArgs e)
        {
            bufCount = 0;
            //dse.Stop();
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

        public delegate byte[] queueFragmentDelegate();

        public void enqueueFragment(queueFragmentDelegate q)
        {
            lock (fq)
            {
                fq.Add(q);
                Monitor.PulseAll(fq);
            }
        }

        public void playFragment(byte[] buffer)
        {
            ++bufCount;
            dse.SubmitBuffer(buffer);
            if (dse.PendingBufferCount >= 10 && dse.State == SoundState.Stopped && bufCount > 50)
            {
                dse.Play();
            }
        }
    }
}
