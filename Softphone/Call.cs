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
    public class Call : ICall
    {
        private AudioIn ai;
        private AudioOut ao;

        public enum CallState
        {
            Init,
            Waiting,
            Linked,
            Up
        }

        public enum CallType
        {
            Incoming,
            Outgoing
        }

        public CallType type;
        Account acc;

        public void Dial(string number, Account ac)
        {
            type = CallType.Outgoing;
            fs.addCall(this);
            IaxFullFrame fnew = new IaxFullFrame();
            fnew.frametype = FrameSender.IAX;
            fnew.subclass = IaxFullFrame.NEW;
            fnew.elements = new InformationElement[] {
                new InformationElement(InformationElement.VERSION,new byte[] {0,2}),
                new InformationElement(InformationElement.CALLEDNUMBER,number),
                new InformationElement(InformationElement.USERNAME,ac.user),
                new InformationElement(InformationElement.CALLINGPRES,new byte[]{0}),
                new InformationElement(InformationElement.CAPABILITY,new byte[]{0,0,0,8}),//alaw
                new InformationElement(InformationElement.FORMAT,new byte[]{0,0,0,8}),//alaw

            };
            acc = ac;
            sendFullFrame(fnew);
        }

        public Call(FrameSender fs, AudioIn ai, AudioOut ao)
        {
            this.fs = fs;
            this.ai = ai;
            this.ao = ao;
            ai.FrameReady += new AudioIn.SampleEventHandler((_s, _args) =>
            {
                int ts = timestamp;
                int ots = 0;
                for (int i = 0; i < _args.data.Length; i += 640)
                {
                    sendVoicFrame(_args.data,i,640,ts+ots);
                    ots += 20;
                }
            });
            fs.onMiniFrame += new FrameSender.MiniFrameEvent((_cn, _tm, _dta, _offs, _cnt) =>
            {
                if (_cn != dstcall) return;
                //byte[] pbuffer = new byte[_cnt];
                byte[] pbuffer = Alaw8.alawtolin16(_dta, _offs, _cnt);
                //Array.Copy(_dta, _offs, pbuffer, 0, _cnt);
                ao.playFragment(pbuffer);
            });


        }
        bool sendFullVoiceFrame = true;
        int oldts = 0;
        public void sendVoicFrame(byte[] vf,int offset,int length,int ts)
        {
            byte[] voice = Alaw8.lin16toalaw(vf, offset, length);
            if (ts < oldts)
            {
                sendFullVoiceFrame = true;
            }
            if (!sendFullVoiceFrame)
                fs.sendMiniFrame(scall, ts, voice);
            else
            {
                IaxFullFrame ifr = new IaxFullFrame();
                ifr.frametype = FrameSender.VOICE;
                ifr.subclass = 8;//alaw
                ifr.data = voice;
                ifr.timestamp = ts;
                sendFullFrame(ifr);
                sendFullVoiceFrame = false;
            }
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} ms", timestamp));
        }

        public const byte HANGUP = 1;
        public const byte RINGING = 3;
        public const byte ANSWER = 4;
        public const byte BUSY = 5;
        public const byte CONGESTION = 8;
        public const byte PROGRESS = 0x0e;
        public const byte PROCEEDING = 0x0f;


        public override void processFrame(IaxFullFrame frm)
        {
            base.processFrame(frm);
            ack(frm);
            switch (frm.frametype)
            {
                case FrameSender.IAX:
                    switch (frm.subclass)
                    {
                        case IaxFullFrame.AUTHREQ:
                            {
                                processAuthFrame(frm);
                                break;
                            }
                        default:
                            break;
                    }
                    break;
                case FrameSender.CONTROL:
                    switch (frm.subclass)
                    {
                        case ANSWER:
                            wlog("answer");
                            ai.Start();
                            break;
                        case HANGUP:
                            wlog("hangup");
                            break;
                        case RINGING:
                            wlog("ringing");
                            break;
                        case BUSY:
                            wlog("busy");
                            break;
                        case CONGESTION:
                            wlog("congest");
                            break;
                        case PROCEEDING:
                            wlog("proceeding");
                            break;
                        case PROGRESS:
                            wlog("progress");
                            break;
                    }
                    break;
            }
        }
        void processAuthFrame(IaxFullFrame frm)
        {
            short authmethods = 0;
            byte[] challenge = null;
            byte[] username;
            foreach (InformationElement ie in frm.elements)
            {
                switch (ie.type)
                {
                    case InformationElement.AUTHMETHODS:
                        {
                            authmethods = FrameSender.mkshort(ie.data[0], ie.data[1]);
                            break;
                        }
                    case InformationElement.CHALLENGE:
                        {
                            challenge = ie.data;
                            break;
                        }
                    case InformationElement.USERNAME:
                        {
                            username = ie.data;
                            break;
                        }
                }
            }
            if (acc.hasPassword && ((authmethods & InformationElement.MD5DIGEST) != 0) && challenge != null)
            {
                IaxFullFrame regreq = new IaxFullFrame();
                regreq.frametype = FrameSender.IAX;
                regreq.subclass = IaxFullFrame.AUTHREP;
                regreq.elements = new InformationElement[] {
                    new InformationElement(InformationElement.USERNAME,acc.user),
                    new InformationElement(InformationElement.MD5RESULT,acc.md5Result(challenge))
                };
                sendFullFrame(regreq);
            }
        }
    }
}
