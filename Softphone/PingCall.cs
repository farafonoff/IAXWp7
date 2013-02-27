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
    public class PingCall : ICall
    {
        public override void processFrame(IaxFullFrame frm)
        {
            base.processFrame(frm);
            ack(frm);
            IaxFullFrame pong = new IaxFullFrame();
            pong.frametype = FrameSender.IAX;
            pong.subclass = IaxFullFrame.PONG;
            pong.timestamp = frm.timestamp;
            sendFullFrame(pong);
        }
    }
}
