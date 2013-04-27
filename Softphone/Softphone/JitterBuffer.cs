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
    class JitterBuffer
    {

        byte[] ibuffer = new byte[650000];
        int iput = 0;
        int iget = 0;
        public int icount = 0;
        bool ifilled = false;
        int prefill = 0;
        bool touched;
        int inbytes;
        int outbytes;

        public double inspeed;
        public double outspeed;
        public double correction = 1.0;
        double overflow;

        #region IWaveProvider Members

        public int Read(byte[] buffer, int offset, int count)
        {
            if (prefill == 0)
            {
                prefill = count;
                ifilled = false;
            }
            lock (this)
            {
                bool underflow = false;
                for (int i = offset; i < (offset + count); i += 2)
                {
                    if (ifilled && icount > 0)
                    {
                        overflow += (correction - 1.0);
                        if (overflow > 1)
                        {
                            iget += 2;
                            iget %= ibuffer.Length;
                            icount -= 2;
                            buffer[i] = ibuffer[iget];
                            buffer[i + 1] = ibuffer[iget + 1];
                            iget += 2;
                            iget %= ibuffer.Length;
                            icount -= 2;
                            overflow -= 1;
                        }
                        else
                            if (overflow < -1)
                            {
                                if (i > 2)
                                {
                                    buffer[i] = buffer[i - 2];
                                    buffer[i + 1] = buffer[i - 1];
                                    overflow += 1;
                                }
                            }
                            else
                            {
                                buffer[i] = ibuffer[iget];
                                buffer[i + 1] = ibuffer[iget + 1];
                                iget += 2;
                                iget %= ibuffer.Length;
                                icount -= 2;
                            }
                    }
                    else
                    {
                        buffer[i] = 0;
                        buffer[i + 1] = 0;
                        underflow = true;
                    }
                }
                if (underflow)
                {
                    if (touched)
                    {
                        //Console.WriteLine("UNDERFLOW");
                        //prefill += 640;
                        //if (prefill > 32000) prefill = 0;
                        ifilled = false;
                    }
                }
                outbytes += count;
                updateSpeeds();
            }
            return count;
        }
        #endregion
        DateTime _begin;

        private void updateSpeeds()
        {
            if (_begin.Year < 1000)
            {
                _begin = DateTime.Now;
                return;
            }
            DateTime now = DateTime.Now;
            double ts = (now - _begin).TotalSeconds;
            if (ts > 2.0)
            {
                inspeed = inbytes / ts;
                outspeed = outbytes / ts;
                correction = ((double)inbytes) / ((double)outbytes);
                double penalty = ((double)(icount - prefill) * 0.00001);
                correction += penalty;
                if (correction > 1.1) correction = 1.1;
                if (correction < 0.9) correction = 0.9;
                inbytes = 0;
                outbytes = 0;
                _begin = now;
            }
        }

        internal void write(byte[] bf, int o, int c)
        {
            lock (this)
            {
                for (int i = o; i < (o + c); ++i)
                {
                    if (icount < ibuffer.Length)
                    {
                        ibuffer[iput] = bf[i];
                        ++iput;
                        iput %= ibuffer.Length;
                        ++icount;
                    }
                    else
                    {
                        //Console.WriteLine("OVERFLOW");
                    }
                }
                if (icount > (16000 * 2 / 2))
                {
                    int df = icount - prefill;
                    icount = prefill;
                    iget += df;
                    iget %= ibuffer.Length;
                }
                if (icount >= prefill) ifilled = true;
                touched = true;
                inbytes += c;
            }
        }
    }

}
