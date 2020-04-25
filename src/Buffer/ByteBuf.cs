using System;

namespace DotNetGameFramework
{
    public class ByteBuf
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// 读下标
        /// </summary>
        public int ReaderIndex { get; set; }

        /// <summary>
        /// 写下标
        /// </summary>
        public int WriterIndex { get; set; }

        /// <summary>
        /// 初始容量
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// 最大容量
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// 引用次数计数器
        /// </summary>
        public int ReferenceCount { get; private set; }

        /// <summary>
        /// deallocate
        /// </summary>
        public Action<ByteBuf> Deallocate;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ByteBuf() : this(256, int.MaxValue)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        public ByteBuf(int capacity): this(capacity, int.MaxValue)
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="maxCapacity"></param>
        public ByteBuf(int capacity, int maxCapacity)
        {
            if (capacity < 0 || capacity > maxCapacity)
            {
                throw new ArgumentException($"capacity:{capacity} maxCapacity:{maxCapacity} excepted capacity greater than zero and less than maxcapacity");
            }

            Data = new byte[capacity];
            ReaderIndex = 0;
            WriterIndex = 0;
            Capacity = capacity;
            MaxCapacity = maxCapacity;
        }


        /// <summary>
        /// 可读字节数
        /// </summary>
        public int ReadableBytes => this.WriterIndex - this.ReaderIndex;

        /// <summary>
        /// 可写字节数
        /// </summary>
        public int WritableBytes => this.Capacity - this.WriterIndex;

        /// <summary>
        /// 跳过指定长度字节
        /// </summary>
        /// <param name="len"></param>
        public void SkipBytes(int len)
        {
            ReaderIndex += len;
            if (ReaderIndex > Capacity)
            {
                ReaderIndex = Capacity;
            }
        }

        /// <summary>
        /// 写bool值
        /// </summary>
        /// <param name="value"></param>
        public void WriteBool(bool value)
        {
            WriteByte(value ? 1 : 0);
        }

        /// <summary>
        /// 写Int
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt(int value)
        {
            EnsureAccessible();
            EnsureWritable(4);
            ByteUtil.SetInt(Data, ReaderIndex, value);
            WriterIndex += 4;
        }

        /// <summary>
        /// 写Long
        /// </summary>
        /// <param name="value"></param>
        public void WriteLong(long value)
        {
            EnsureAccessible();
            EnsureWritable(8);
            ByteUtil.SetLong(Data, ReaderIndex, value);
            WriterIndex += 8;
        }

        /// <summary>
        /// 写Float型
        /// </summary>
        /// <param name="value"></param>
        public void WriteFloat(float value)
        {
            WriteInt(BitConverter.SingleToInt32Bits(value));
        }

        /// <summary>
        /// 写Double
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            WriteLong(BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// 写字节
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(int value)
        {
            EnsureWritable(1);
            ByteUtil.SetByte(Data, WriterIndex, value);
            WriterIndex += 1;
        }

        /// <summary>
        /// 写字节数组
        /// </summary>
        /// <param name="array"></param>
        public void WriteBytes(byte[] array)
        {
            WriteBytes(array, 0, array.Length);
        }

        /// <summary>
        /// 写字节数组
        /// </summary>
        /// <param name="array"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        public void WriteBytes(byte[] array, int srcIndex, int len)
        {
            EnsureAccessible();
            EnsureWritable(len);
            Array.Copy(array, srcIndex, Data, WriterIndex, len);
            WriterIndex += len;
        }

        /// <summary>
        /// 写字节数组
        /// </summary>
        /// <param name="buff"></param>
        public void WriteBytes(ByteBuf buff)
        {
            WriteBytes(buff, buff.ReaderIndex, buff.ReadableBytes);
        }

        /// <summary>
        /// 写字节数组
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="srcIndex"></param>
        /// <param name="len"></param>
        public void WriteBytes(ByteBuf buff, int srcIndex, int len)
        {
            WriteBytes(buff.Data, srcIndex, len);
        }

        /// <summary>
        /// 读取bool值
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return ReadByte() != 0;
        }

        /// <summary>
        /// 读取byte字节
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            byte v = GetByte();
            ReaderIndex += 1;

            return v;
        }

        /// <summary>
        /// 读取int值
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            int v = GetInt();
            ReaderIndex += 4;

            return v;
        }

        /// <summary>
        /// 读取Long值
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            long v = GetLong();
            ReaderIndex += 8;

            return v;
        }

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <returns></returns>
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble(ReadLong());
        }

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <returns></returns>
        public float ReadFloat()
        {
            return BitConverter.Int32BitsToSingle(ReadInt());
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="dest"></param>
        public void ReadBytes(byte[] dest)
        {
            ReadBytes(dest, 0, dest.Length);
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="dstIndex"></param>
        /// <param name="len"></param>
        public void ReadBytes(byte[] dest, int dstIndex, int len)
        {
            GetBytes(dest, dstIndex, len);
            ReaderIndex += len;
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="len"></param>
        public ByteBuf ReadBytes(int len)
        {
            ByteBuf buff = new ByteBuf(len);
            ReadBytes(buff);

            return buff;
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name=""></param>
        /// <param name="dstIndex"></param>
        /// <param name="len"></param>
        public void ReadBytes(ByteBuf dest)
        {
            int len = dest.WritableBytes;
            Array.Copy(Data, ReaderIndex, dest.Data, dest.WriterIndex, dest.WritableBytes);
            dest.WriterIndex += len;
            ReaderIndex += len;
        }

        /// <summary>
        /// 读取bool值
        /// </summary>
        /// <returns></returns>
        public bool GetBool()
        {
            return GetByte() != 0;
        }

        /// <summary>
        /// 读取byte字节
        /// </summary>
        /// <returns></returns>
        public byte GetByte()
        {
            return ByteUtil.GetByte(Data, ReaderIndex);
        }

        /// <summary>
        /// 读取int值
        /// </summary>
        /// <returns></returns>
        public int GetInt()
        {
            return ByteUtil.GetInt(Data, ReaderIndex);
        }

        /// <summary>
        /// 读取Long值
        /// </summary>
        /// <returns></returns>
        public long GetLong()
        {
            return ByteUtil.GetLong(Data, ReaderIndex);
        }

        /// <summary>
        /// 读取Double
        /// </summary>
        /// <returns></returns>
        public double GetDouble()
        {
            return BitConverter.Int64BitsToDouble(GetLong());
        }

        /// <summary>
        /// 读取Float
        /// </summary>
        /// <returns></returns>
        public float GetFloat()
        {
            return BitConverter.Int32BitsToSingle(GetInt());
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="dest"></param>
        public void GetBytes(byte[] dest)
        {
            GetBytes(dest, 0, dest.Length);
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="dstIndex"></param>
        /// <param name="len"></param>
        public void GetBytes(byte[] dest, int dstIndex, int len)
        {
            Array.Copy(Data, ReaderIndex, dest, dstIndex, len);
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name="buff"></param>
        public ByteBuf GetBytes(int len)
        {
            ByteBuf buff = new ByteBuf(len);
            GetBytes(buff);

            return buff;
        }

        /// <summary>
        /// 读取字节
        /// </summary>
        /// <param name=""></param>
        public void GetBytes(ByteBuf buff)
        {
            Array.Copy(Data, ReaderIndex, buff.Data, buff.WriterIndex, buff.WritableBytes);
        }

        /// <summary>
        /// 引用+1
        /// </summary>
        public void Retain()
        {
            ReferenceCount++;
        }

        public void Release()
        {
            ReferenceCount--;
            if (ReferenceCount == 0)
            {
                ReaderIndex = 0;
                WriterIndex = 0;
                Deallocate?.Invoke(this);
            }
        }

        /// <summary>
        /// 检查可访问性
        /// </summary>
        protected void EnsureAccessible()
        {
            if (this.ReferenceCount == 0)
            {
                throw new Exception(String.Format("illeage reference count {0}", ReferenceCount));
            }
        }

        /// <summary>
        /// 检查是否可写
        /// </summary>
        /// <param name="minWritableBytes"></param>
        protected void EnsureWritable(int minWritableBytes)
        {
            if (minWritableBytes < 0)
            {
                throw new ArgumentException(String.Format("minWritableBytes {0}, excepted >= 0", minWritableBytes));
            }

            if (minWritableBytes <= WritableBytes)
            {
                return;
            }

            if (minWritableBytes > MaxCapacity - WriterIndex)
            {
                throw new IndexOutOfRangeException($"writerindex({WriterIndex}) + minWritableBytes({minWritableBytes}) exceeds maxCapacity({MaxCapacity})");
            }

            int minNewCapacity = WriterIndex + minWritableBytes;
            int newCapacity = calculateNewCapacity(minNewCapacity);
            int fastCapacity = WriterIndex + WritableBytes;
            if (newCapacity > fastCapacity && minNewCapacity <= fastCapacity)
            {
                newCapacity = fastCapacity;
            }

            AdjustCapacity(newCapacity);
        }

        /// <summary>
        /// 重新调整容量大小
        /// </summary>
        /// <param name="newCapacity"></param>
        private void AdjustCapacity(int newCapacity)
        {
            int oldCapacity = Capacity;
            byte[] oldArray = Data;
            if (newCapacity > oldCapacity)
            {
                byte[] newArray = new byte[newCapacity];
                Array.Copy(oldArray, newArray, oldCapacity);
                Capacity = newCapacity;
            }
        }

        /// <summary>
        /// 计算新大小
        /// </summary>
        /// <param name="minNewCapacity"></param>
        /// <returns></returns>
        private int calculateNewCapacity(int minNewCapacity)
        {
            if (minNewCapacity > MaxCapacity)
            {
                throw new ArgumentException($"minNewCapacity:{minNewCapacity}, excepted not greater than maxCapacity:{MaxCapacity}");
            }

            int newCapacity = 64;
            while (newCapacity < minNewCapacity)
            {
                newCapacity <<= 1;
            }

            return Math.Min(newCapacity, MaxCapacity);
        }


    }
}

