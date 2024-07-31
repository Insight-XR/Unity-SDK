using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace InsightDesk
{
    public class InsightBuffer
    {
        public IntPtr unmanagedBuffer;
        public int ofs = 0;
        public int length = 0;

        private bool _hasFreedBuffer = false;

        public InsightBuffer(int length)
        {
            unmanagedBuffer = Marshal.AllocHGlobal(length);
            this.length = length;
        }

        ~InsightBuffer()
        {
            Free();
        }

        // same as Write(string value) except it treats string like char array and does not include 7 bit encoded length or null terminating character
        public void WriteCharArray(string value)
        {
            var bytesRemaining = length - ofs;
            if (value.Length > bytesRemaining)
            {
                throw new InvalidOperationException("buffer does not have enough space to write value");
            }

            for (int i = 0; i < value.Length; i++)
            {
                Marshal.WriteByte(unmanagedBuffer, ofs + i, (byte)value[i]);
            }

            ofs += value.Length;
        }

        public void Write(string value)
        {
            if (value.Length > 100)
            {
                throw new ArgumentException("string cannot be longer than 100 characters.");
            }

            var size = value.Length + 1;
            var bytesRemaining = length - ofs;
            if (size > bytesRemaining)
            {
                throw new InvalidOperationException("buffer does not have enough space to write value");
            }

            byte length7BitEncodedInt = (byte)value.Length;
            Marshal.WriteByte(unmanagedBuffer, ofs, length7BitEncodedInt);
            for (int i = 0; i < value.Length; i++)
            {
                Marshal.WriteByte(unmanagedBuffer, ofs + 1 + i, (byte)value[i]);
            }

            ofs += size;
        }

        public void Write<T>(T value)
        {
            var size = Marshal.SizeOf(typeof(T));
            var bytesRemaining = length - ofs;
            if (size > bytesRemaining)
            {
                throw new InvalidOperationException("buffer does not have enough space to write value");
            }

            switch (size)
            {
                case 1:
                    Marshal.WriteByte(unmanagedBuffer, ofs, UnsafeUtility.As<T, byte>(ref value));
                    break;
                case 2:
                    Marshal.WriteInt16(unmanagedBuffer, ofs, UnsafeUtility.As<T, short>(ref value));
                    break;
                case 4:
                    Marshal.WriteInt32(unmanagedBuffer, ofs, UnsafeUtility.As<T, int>(ref value));
                    break;
                case 8:
                    Marshal.WriteInt64(unmanagedBuffer, ofs, UnsafeUtility.As<T, long>(ref value));
                    break;
            }

            ofs += size;
        }

        public void Free()
        {
            if (!_hasFreedBuffer)
            {
                _hasFreedBuffer = true;
                Marshal.FreeHGlobal(unmanagedBuffer);
            }
        }
    }
}