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
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Softphone
{
    public class FrameSender
    {
        Timer secondsTimer;

        Dictionary<ushort, ICall> calls = new Dictionary<ushort, ICall>();
        public ushort getUnusedCall()
        {
            Random rnd = new Random();
            ushort cn = 0;
            do
            {
                cn = (ushort)(rnd.Next(10000) + 10000);
            } while (calls.ContainsKey(cn));
            return cn;
        }
        public void addCall(ICall c)
        {
            lock (calls)
            {
                ushort cn = getUnusedCall();
                c.scall = cn;
                calls[cn] = c;
            }
        }
        public void delCall(ICall c)
        {
            lock (calls)
            {
                ushort cn = c.scall;
                calls.Remove(cn);
            }
        }
        public System.Net.Sockets.Socket _socket;
        public FrameSender(Socket soc)
        {
            _socket = soc;
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.Completed += new EventHandler<SocketAsyncEventArgs>(sea_Completed);
            sea.SetBuffer(new byte[1400], 0, 1400);
            if (!_socket.ReceiveAsync(sea))
            {
                sea_Completed(_socket, sea);
            }
            secondsTimer = new Timer(pullCalls, this, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        }
        public void pullCalls(object stateInfo)
        {
            ICall[] tmp;
            lock(calls)
            {
                tmp = new ICall[calls.Count];
                calls.Values.CopyTo(tmp, 0);
            }
            foreach(var call in tmp)
            {
                call.timer(1);
            }
        }

        void sea_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0)
            {
                byte[] data = e.Buffer;
                if ((data[0] & 0x80) != 0)
                {
                    processFullFrame(data, e.BytesTransferred);
                }
                else
                {
                    processMiniFrame(data, e.BytesTransferred);
                }
            }
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.Completed += new EventHandler<SocketAsyncEventArgs>(sea_Completed);
            sea.SetBuffer(e.Buffer, 0, e.Buffer.Length);
            _socket.ReceiveAsync(sea);
        }

        private void processMiniFrame(byte[] data, int p)
        {
            if (onMiniFrame != null)
            {
                int callno = mkshort(data[0], data[1]);
                int timestamp = mkshort(data[2], data[3]);
                onMiniFrame(callno, timestamp, data, 4, p - 4);
            }
        }

        public delegate void MiniFrameEvent(int callno, int timestamp, byte[] data, int offset, int count);

        public event MiniFrameEvent onMiniFrame;
        public const byte VOICE = 2;
        public const byte IAX = 6;
        public const byte CONTROL = 4;

        private void processFullFrame(byte[] data, int p)
        {
            IaxFullFrame result = new IaxFullFrame();
            result.sourcecall = mkshort(data[0] & 0x7F, data[1]);
            result.dstcall = mkshort(data[2] & 0x7F, data[3]);
            result.retransmission = (data[2] & 0x80) != 0;
            result.timestamp = mkint(data[4], data[5], data[6], data[7]);
            result.oseq = data[8];
            result.iseq = data[9];
            result.frametype = data[10];
            result.subclass = data[11];
            int di = 12;
            if (result.frametype == FrameSender.IAX||result.frametype==FrameSender.CONTROL)
            {//IAX frame
                List<InformationElement> elementz = new List<InformationElement>();
                while (di < p)
                {
                    InformationElement ie = new InformationElement();
                    ie.type = data[di];
                    ++di;
                    ie.datalen = data[di];
                    ie.data = new byte[ie.datalen];
                    ++di;
                    for (int cc = 0; cc < ie.datalen; ++cc)
                    {
                        ie.data[cc] = data[di + cc];
                    }
                    di += ie.datalen;
                    elementz.Add(ie);
                }
                result.elements = elementz.ToArray();
                if (result.dstcall == 0)
                {
                    //create new call
                    newCall(result);
                }
                if (calls.ContainsKey(result.dstcall))
                {
                    calls[result.dstcall].processFrame(result);
                }
            }
            else if (result.frametype==FrameSender.VOICE)
            {
                if (calls.ContainsKey(result.dstcall))
                {
                    onMiniFrame(result.dstcall, result.timestamp, data, di, p - di);
                }
            }
        }

        private void newCall(IaxFullFrame result)
        {
            switch (result.subclass)
            {
                case IaxFullFrame.POKE:
                case IaxFullFrame.PING:
                    PingCall pc = new PingCall();
                    pc.fs = this;
                    pc.scall = getUnusedCall();
                    pc.processFrame(result);
                    break;
            }
        }

        private ushort mkshort(int hib, byte lob)
        {
            return (ushort)((hib << 8) | lob);
        }
        private int mkint(byte b1, byte b2, byte b3, byte b4)
        {
            return (b1 << 24) | (b2 << 16) | (b3 << 8) | b4;
        }
        public static byte hib(ushort v)
        {
            return (byte)(v >> 8);
        }
        public static byte lob(ushort v)
        {
            return (byte)(v & 255);
        }
        public static byte gib(int v, int b)
        {
            return (byte)((v >> (b * 8)) & 0xFF);
        }
        public static short mkshort(byte hib, byte lob)
        {
            return (short)((hib << 8) | lob);
        }
        public void sendMiniFrame(ushort callno, int timestamp, byte[] data)
        {
            byte[] packet = new byte[4 + data.Length];
            packet[0] = (byte)(hib(callno) & 0x7F);
            packet[1] = lob(callno);
            packet[2] = gib(timestamp,1);
            packet[3] = gib(timestamp,0);
            Array.Copy(data, 0, packet, 4, data.Length);
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.SetBuffer(packet, 0, packet.Length);
            _socket.SendAsync(sea);
        }

        internal void sendFullFrame(IaxFullFrame regreq)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add((byte)(hib(regreq.sourcecall) | 0x80));
            bytes.Add(lob(regreq.sourcecall));
            bytes.Add((byte)(hib(regreq.dstcall) | (regreq.retransmission ? 0x80 : 0x00)));
            bytes.Add(lob(regreq.dstcall));
            for (int bt = 0; bt < 4; ++bt)
                bytes.Add(gib(regreq.timestamp, 3-bt));
            bytes.Add(regreq.oseq);
            bytes.Add(regreq.iseq);
            bytes.Add(regreq.frametype);
            bytes.Add(regreq.subclass);
            if (regreq.elements != null && regreq.elements.Length > 0)
            {
                foreach (InformationElement ie in regreq.elements)
                {
                    bytes.Add(ie.type);
                    bytes.Add(ie.datalen);
                    foreach (byte ieb in ie.data)
                    {
                        bytes.Add(ieb);
                    }
                }
            }
            if (regreq.data != null && regreq.data.Length > 0)
            {
                foreach (byte b in regreq.data)
                {
                    bytes.Add(b);
                }
            }
            byte[] packet = bytes.ToArray();
            SocketAsyncEventArgs sea = new SocketAsyncEventArgs();
            sea.SetBuffer(packet, 0, packet.Length);
            _socket.SendAsync(sea);
        }
    }
}
