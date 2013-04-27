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
    public class Registration : ICall
    {
        RegState state;
        Account acc;
        public Registration(FrameSender fs)
        {
            this.fs = fs;
            state = RegState.Unregistered;
            fs.addCall(this);
        }
        public enum RegState
        {
            Unregistered,
            RegSent,
            Registered,
            Releasing,
            NoAuth,
            Rejected
        }
        public void Register(Account acc)
        {
            this.acc = acc;
            //send regreq
            IaxFullFrame regreq = new IaxFullFrame();
            regreq.frametype = FrameSender.IAX;
            regreq.subclass = IaxFullFrame.REGREQ;
            regreq.elements = new InformationElement[] {
                new InformationElement(InformationElement.USERNAME,acc.user)
            };
            sendFullFrame(regreq);
            state = RegState.RegSent;
        }
        public override void processFrame(IaxFullFrame frm)
        {
            base.processFrame(frm);
            switch (frm.subclass)
            {
                case IaxFullFrame.REGAUTH:
                    {
                        processAuthFrame(frm);
                        break;
                    }
                case IaxFullFrame.REGACK:
                    {
                        ack(frm);
                        state = RegState.Registered;
                        break;
                    }
                case IaxFullFrame.REGREJ:
                    {
                        ack(frm);
                        state = RegState.Rejected;
                        break;
                    }
            }
        }
        public int reregTimeout = 30;
        public override void timer(int p)
        {
            base.timer(p);
            lock (this)
            {
                switch (state)
                {
                    case RegState.Registered:

                        reregTimeout -= p;
                        if (reregTimeout < 0)
                        {
                            fs.delCall(this);
                            Registration rereg = new Registration(fs);
                            rereg.Register(acc);
                        }
                        break;

                }
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
            if (acc.hasPassword&&((authmethods&InformationElement.MD5DIGEST)!=0)&&challenge!=null)
            {
                IaxFullFrame regreq = new IaxFullFrame();
                regreq.frametype = FrameSender.IAX;
                regreq.subclass = IaxFullFrame.REGREQ;
                regreq.elements = new InformationElement[] {
                    new InformationElement(InformationElement.USERNAME,acc.user),
                    new InformationElement(InformationElement.MD5RESULT,acc.md5Result(challenge))
                };
                sendFullFrame(regreq);                
            }
        }
    }
}
