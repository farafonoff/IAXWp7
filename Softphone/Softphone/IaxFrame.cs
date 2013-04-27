using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Softphone
{
    /*
     *| 0x0d | REGREQ    | Registration request                    |
      |      |           |                                         |
      | 0x0e | REGAUTH   | Registration authentication             |
      |      |           |                                         |
      | 0x0f | REGACK    | Registration acknowledgement            |
      |      |           |                                         |
      | 0x10 | REGREJ    | Registration reject                     |
      |      |           |                                         |
      | 0x11 | REGREL    | Registration release                    |
      |      |           |                       
     * */
    public struct IaxFullFrame
    {
        public const byte NEW = 0x01;
        public const byte PING = 0x02;
        public const byte PONG = 0x03;
        public const byte ACK = 0x04;
        public const byte AUTHREQ = 0x08;
        public const byte AUTHREP = 0x09;
        public const byte POKE = 0x1e;
        /* REG */
        public const byte REGREQ = 0x0d;
        public const byte REGAUTH = 0x0e;
        public const byte REGACK = 0x0f;
        public const byte REGREJ = 0x10;
        public const byte REGREL = 0x11;

        public ushort sourcecall;
        public ushort dstcall;
        public bool retransmission;
        public byte iseq;
        public byte oseq;
        public byte frametype;
        public byte subclass;
        public int timestamp;

        public InformationElement[] elements;
        public byte[] data;
    }
}
