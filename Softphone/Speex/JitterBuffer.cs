namespace Ozeki.Media.Codec.Speex.Implementation
{
    using System;
    using System.Runtime.InteropServices;

    internal class JitterBuffer
    {
        private TimingBuffer[] _tb = new TimingBuffer[3];
        private long[] arrival = new long[200];
        private bool auto_adjust;
        public int auto_tradeoff;
        private int buffer_margin;
        private long buffered;
        private int concealment_size;
        public int delay_step;
        public Action<byte[]> DestroyBufferCallback;
        private int interp_requested;
        public const int JITTER_BUFFER_BAD_ARGUMENT = -2;
        public const int JITTER_BUFFER_INSERTION = 2;
        public const int JITTER_BUFFER_INTERNAL_ERROR = -1;
        public const int JITTER_BUFFER_MISSING = 1;
        public const int JITTER_BUFFER_OK = 0;
        private long last_returned_timestamp;
        private int late_cutoff;
        public int latency_tradeoff;
        private int lost_count;
        private const int MAX_BUFFER_SIZE = 200;
        private const int MAX_BUFFERS = 3;
        private int max_late_rate;
        private const int MAX_TIMINGS = 40;
        private long next_stop;
        private JitterBufferPacket[] packets = new JitterBufferPacket[200];
        private long pointer_timestamp;
        private bool reset_state;
        private int subwindow_size;
        private TimingBuffer[] timeBuffers = new TimingBuffer[3];
        private const int TOP_DELAY = 40;
        public int window_size;

        private byte[] AllocBuffer(long size)
        {
            return new byte[size];
        }

        private short ComputeOptDelay()
        {
            int num;
            float num6;
            short num2 = 0;
            int num3 = 0x7fffffff;
            int num4 = 0;
            int[] numArray = new int[3];
            bool flag = false;
            int num7 = 0;
            int num8 = 0;
            TimingBuffer[] bufferArray = this._tb;
            int num5 = 0;
            for (num = 0; num < 3; num++)
            {
                num5 += bufferArray[num].curr_count;
            }
            if (num5 == 0)
            {
                return 0;
            }
            if (this.latency_tradeoff != 0)
            {
                num6 = (this.latency_tradeoff * 100f) / ((float) num5);
            }
            else
            {
                num6 = (this.auto_tradeoff * this.window_size) / num5;
            }
            num = 0;
            while (num < 3)
            {
                numArray[num] = 0;
                num++;
            }
            for (num = 0; num < 40; num++)
            {
                int index = -1;
                int x = 0x7fff;
                for (int i = 0; i < 3; i++)
                {
                    if ((numArray[i] < bufferArray[i].filled) && (bufferArray[i].timing[numArray[i]] < x))
                    {
                        index = i;
                        x = bufferArray[i].timing[numArray[i]];
                    }
                }
                if (index == -1)
                {
                    break;
                }
                if (num == 0)
                {
                    num8 = x;
                }
                num7 = x;
                x = RoundDown(x, this.delay_step);
                numArray[index]++;
                int num13 = -x + ((int) (num6 * num4));
                if (num13 < num3)
                {
                    num3 = num13;
                    num2 = (short) x;
                }
                num4++;
                if ((x >= 0) && !flag)
                {
                    flag = true;
                    num4 += 4;
                }
            }
            int num9 = num7 - num8;
            this.auto_tradeoff = 1 + (num9 / 40);
            if ((num5 < 40) && (num2 > 0))
            {
                return 0;
            }
            return num2;
        }

        private void FreeBuffer(byte[] buffer)
        {
        }

        public int Get(ref JitterBufferPacket packet, int desired_span, out int start_offset)
        {
            int num;
            start_offset = 0;
            if (this.reset_state)
            {
                bool flag = false;
                long timestamp = 0L;
                for (num = 0; num < 200; num++)
                {
                    if ((this.packets[num].data != null) && (!flag || (this.packets[num].timestamp < timestamp)))
                    {
                        timestamp = this.packets[num].timestamp;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    packet.timestamp = 0L;
                    packet.span = this.interp_requested;
                    return 1;
                }
                this.reset_state = false;
                this.pointer_timestamp = timestamp;
                this.next_stop = timestamp;
            }
            this.last_returned_timestamp = this.pointer_timestamp;
            if (this.interp_requested != 0)
            {
                packet.timestamp = this.pointer_timestamp;
                packet.span = this.interp_requested;
                this.pointer_timestamp += this.interp_requested;
                packet.len = 0;
                this.interp_requested = 0;
                this.buffered = packet.span - desired_span;
                return 2;
            }
            num = 0;
            while (num < 200)
            {
                if (((this.packets[num].data != null) && (this.packets[num].timestamp == this.pointer_timestamp)) && ((this.packets[num].timestamp + this.packets[num].span) >= (this.pointer_timestamp + desired_span)))
                {
                    break;
                }
                num++;
            }
            if (num == 200)
            {
                num = 0;
                while (num < 200)
                {
                    if (((this.packets[num].data != null) && (this.packets[num].timestamp <= this.pointer_timestamp)) && ((this.packets[num].timestamp + this.packets[num].span) >= (this.pointer_timestamp + desired_span)))
                    {
                        break;
                    }
                    num++;
                }
            }
            if (num == 200)
            {
                num = 0;
                while (num < 200)
                {
                    if (((this.packets[num].data != null) && (this.packets[num].timestamp <= this.pointer_timestamp)) && ((this.packets[num].timestamp + this.packets[num].span) > this.pointer_timestamp))
                    {
                        break;
                    }
                    num++;
                }
            }
            if (num == 200)
            {
                bool flag2 = false;
                long num5 = 0L;
                long span = 0L;
                int num7 = 0;
                num = 0;
                while (num < 200)
                {
                    if ((((this.packets[num].data != null) && (this.packets[num].timestamp < (this.pointer_timestamp + desired_span))) && (this.packets[num].timestamp >= this.pointer_timestamp)) && ((!flag2 || (this.packets[num].timestamp < num5)) || ((this.packets[num].timestamp == num5) && (this.packets[num].span > span))))
                    {
                        num5 = this.packets[num].timestamp;
                        span = this.packets[num].span;
                        num7 = num;
                        flag2 = true;
                    }
                    num++;
                }
                if (flag2)
                {
                    num = num7;
                }
            }
            if (num != 200)
            {
                this.lost_count = 0;
                if (this.arrival[num] != 0L)
                {
                    this.UpdateTimings((((int) this.packets[num].timestamp) - ((int) this.arrival[num])) - this.buffer_margin);
                }
                if (this.DestroyBufferCallback != null)
                {
                    packet.data = this.packets[num].data;
                    packet.len = this.packets[num].len;
                }
                else
                {
                    if (this.packets[num].len <= packet.len)
                    {
                        packet.len = this.packets[num].len;
                    }
                    for (long i = 0L; i < packet.len; i += 1L)
                    {
                        packet.data[(int) ((IntPtr) i)] = this.packets[num].data[(int) ((IntPtr) i)];
                    }
                    this.FreeBuffer(this.packets[num].data);
                }
                this.packets[num].data = null;
                int num8 = ((int) this.packets[num].timestamp) - ((int) this.pointer_timestamp);
                if (start_offset != 0)
                {
                    start_offset = num8;
                }
                packet.timestamp = this.packets[num].timestamp;
                this.last_returned_timestamp = packet.timestamp;
                packet.span = this.packets[num].span;
                packet.sequence = this.packets[num].sequence;
                packet.user_data = this.packets[num].user_data;
                packet.len = this.packets[num].len;
                this.pointer_timestamp = this.packets[num].timestamp + this.packets[num].span;
                this.buffered = packet.span - desired_span;
                if (start_offset != 0)
                {
                    this.buffered += (long) start_offset;
                }
                return 0;
            }
            this.lost_count++;
            short num3 = this.ComputeOptDelay();
            if (num3 < 0)
            {
                this.ShiftTimings((short)-num3);
                packet.timestamp = this.pointer_timestamp;
                packet.span = -num3;
                packet.len = 0;
                this.buffered = packet.span - desired_span;
                return 2;
            }
            packet.timestamp = this.pointer_timestamp;
            desired_span = RoundDown(desired_span, this.concealment_size);
            packet.span = desired_span;
            this.pointer_timestamp += desired_span;
            packet.len = 0;
            this.buffered = packet.span - desired_span;
            return 1;
        }

        public void Init(int step_size)
        {
            int num;
            for (num = 0; num < 200; num++)
            {
                this.packets[num].data = null;
            }
            for (num = 0; num < 3; num++)
            {
                this._tb[num] = new TimingBuffer();
            }
            this.delay_step = step_size;
            this.concealment_size = step_size;
            this.buffer_margin = 0;
            this.late_cutoff = 50;
            this.DestroyBufferCallback = null;
            this.latency_tradeoff = 0;
            this.auto_adjust = true;
            int maxLateRate = 4;
            this.SetMaxLateRate(maxLateRate);
            this.Reset();
        }

        public void Put(JitterBufferPacket packet)
        {
            int num;
            bool flag;
            if (!this.reset_state)
            {
                for (num = 0; num < 200; num++)
                {
                    if ((this.packets[num].data != null) && ((this.packets[num].timestamp + this.packets[num].span) <= this.pointer_timestamp))
                    {
                        if (this.DestroyBufferCallback != null)
                        {
                            this.DestroyBufferCallback(this.packets[num].data);
                        }
                        else
                        {
                            this.FreeBuffer(this.packets[num].data);
                        }
                        this.packets[num].data = null;
                    }
                }
            }
            if (!this.reset_state && (packet.timestamp < this.next_stop))
            {
                this.UpdateTimings((((int) packet.timestamp) - ((int) this.next_stop)) - this.buffer_margin);
                flag = true;
            }
            else
            {
                flag = false;
            }
            if (this.lost_count > 20)
            {
                this.Reset();
            }
            if (this.reset_state || (((packet.timestamp + packet.span) + this.delay_step) >= this.pointer_timestamp))
            {
                int num2;
                num = 0;
                while (num < 200)
                {
                    if (this.packets[num].data == null)
                    {
                        break;
                    }
                    num++;
                }
                if (num == 200)
                {
                    long timestamp = this.packets[0].timestamp;
                    num = 0;
                    for (num2 = 1; num2 < 200; num2++)
                    {
                        if ((this.packets[num].data == null) || (this.packets[num2].timestamp < timestamp))
                        {
                            timestamp = this.packets[num2].timestamp;
                            num = num2;
                        }
                    }
                    if (this.DestroyBufferCallback != null)
                    {
                        this.DestroyBufferCallback(this.packets[num].data);
                    }
                    else
                    {
                        this.FreeBuffer(this.packets[num].data);
                    }
                    this.packets[num].data = null;
                }
                if (this.DestroyBufferCallback != null)
                {
                    this.packets[num].data = packet.data;
                }
                else
                {
                    this.packets[num].data = this.AllocBuffer((long) packet.len);
                    for (num2 = 0; num2 < packet.len; num2++)
                    {
                        this.packets[num].data[num2] = packet.data[num2];
                    }
                }
                this.packets[num].timestamp = packet.timestamp;
                this.packets[num].span = packet.span;
                this.packets[num].len = packet.len;
                this.packets[num].sequence = packet.sequence;
                this.packets[num].user_data = packet.user_data;
                if (this.reset_state || flag)
                {
                    this.arrival[num] = 0L;
                }
                else
                {
                    this.arrival[num] = this.next_stop;
                }
            }
        }

        private void Reset()
        {
            int num;
            for (num = 0; num < 200; num++)
            {
                if (this.packets[num].data != null)
                {
                    if (this.DestroyBufferCallback != null)
                    {
                        this.DestroyBufferCallback(this.packets[num].data);
                    }
                    else
                    {
                        this.FreeBuffer(this.packets[num].data);
                    }
                    this.packets[num].data = null;
                }
            }
            this.pointer_timestamp = 0L;
            this.next_stop = 0L;
            this.reset_state = true;
            this.lost_count = 0;
            this.buffered = 0L;
            this.auto_tradeoff = 0x7d00;
            for (num = 0; num < 3; num++)
            {
                this._tb[num].Init();
                this.timeBuffers[num] = this._tb[num];
            }
        }

        private static int RoundDown(int x, int step)
        {
            if (x < 0)
            {
                return ((((x - step) + 1) / step) * step);
            }
            return ((x / step) * step);
        }

        private void SetMaxLateRate(int maxLateRate)
        {
            this.max_late_rate = maxLateRate;
            this.window_size = 0xfa0 / this.max_late_rate;
            this.subwindow_size = this.window_size / 3;
        }

        private void ShiftTimings(short amount)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < this.timeBuffers[i].filled; j++)
                {
                    this.timeBuffers[i].timing[j] += amount;
                }
            }
        }

        public void Tick()
        {
            if (this.auto_adjust)
            {
                this.UpdateDelay();
            }
            if (this.buffered >= 0L)
            {
                this.next_stop = this.pointer_timestamp - this.buffered;
            }
            else
            {
                this.next_stop = this.pointer_timestamp;
            }
            this.buffered = 0L;
        }

        private int UpdateDelay()
        {
            short num = this.ComputeOptDelay();
            if (num < 0)
            {
                this.ShiftTimings((short)-num);
                this.pointer_timestamp += num;
                this.interp_requested = -num;
                return num;
            }
            if (num > 0)
            {
                this.ShiftTimings((short)-num);
                this.pointer_timestamp += num;
            }
            return num;
        }

        private void UpdateTimings(int timing)
        {
            if (timing < -32768)
            {
                timing = -32768;
            }
            if (timing > 0x7fff)
            {
                timing = 0x7fff;
            }
            short num = (short) timing;
            if (this.timeBuffers[0].curr_count >= this.subwindow_size)
            {
                TimingBuffer buffer = this.timeBuffers[2];
                for (int i = 2; i >= 1; i--)
                {
                    this.timeBuffers[i] = this.timeBuffers[i - 1];
                }
                this.timeBuffers[0] = buffer;
                this.timeBuffers[0].Init();
            }
            this.timeBuffers[0].Add(num);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JitterBufferPacket
        {
            public byte[] data;
            public int len;
            public long timestamp;
            public long span;
            public long sequence;
            public long user_data;
        }

        private class TimingBuffer
        {
            public short[] counts = new short[40];
            public int curr_count;
            public int filled;
            public int[] timing = new int[40];

            internal void Add(short timing)
            {
                if ((this.filled >= 40) && (timing >= this.timing[this.filled - 1]))
                {
                    this.curr_count++;
                }
                else
                {
                    int index = 0;
                    while ((index < this.filled) && (timing >= this.timing[index]))
                    {
                        index++;
                    }
                    if (index < this.filled)
                    {
                        int length = this.filled - index;
                        if (this.filled == 40)
                        {
                            length--;
                        }
                        Array.Copy(this.timing, index, this.timing, index + 1, length);
                        Array.Copy(this.counts, index, this.counts, index + 1, length);
                    }
                    this.timing[index] = timing;
                    this.counts[index] = (short) this.curr_count;
                    this.curr_count++;
                    if (this.filled < 40)
                    {
                        this.filled++;
                    }
                }
            }

            internal void Init()
            {
                this.filled = 0;
                this.curr_count = 0;
            }
        }
    }
}

