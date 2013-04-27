using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Softphone
{
    public abstract class ICall
    {
        public ICall()
        {
            initTime = DateTime.Now;
        }
        public FrameSender fs;
        public virtual void processFrame(IaxFullFrame frm)
        {
            dstcall = frm.sourcecall;
            ++iseq;
        }
        DateTime initTime;
        public ushort scall = 0;
        public ushort dstcall = 0;
        protected byte iseq;
        protected byte oseq;
        public int timestamp
        {
            get
            {
                return (int)(((long)(DateTime.Now - initTime).TotalMilliseconds));
            }
        }

        protected void sendFullFrame(IaxFullFrame frame)
        {
            frame.dstcall = dstcall;
            frame.sourcecall = scall;
            frame.iseq = iseq;
            frame.oseq = oseq;
            ++oseq;
            if (frame.timestamp == 0)
            {
                frame.timestamp = timestamp;
            }
            fs.sendFullFrame(frame);
        }
        protected void ack(IaxFullFrame onwhat)
        {
            IaxFullFrame fr = new IaxFullFrame();
            fr.timestamp = onwhat.timestamp;
            fr.frametype = FrameSender.IAX;
            fr.subclass = IaxFullFrame.ACK;
            fr.dstcall = dstcall;
            fr.sourcecall = scall;
            fr.iseq = iseq;
            fr.oseq = oseq;
            fs.sendFullFrame(fr);
        }

        public virtual void timer(int p)
        {
            
        }

        public delegate void LogEvent(int scall, int dcall, string text);

        public event LogEvent log;

        protected void wlog(string s)
        {
            if (log != null)
            {
                log(scall, dstcall, s);
            }
        }
    }
}
