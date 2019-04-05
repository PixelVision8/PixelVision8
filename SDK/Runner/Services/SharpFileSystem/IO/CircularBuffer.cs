using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SharpFileSystem.IO
{
    // CircularBuffer from http://circularbuffer.codeplex.com/.
    public class CircularBuffer<T> : ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        private T[] buffer;
        private int capacity;
        private int head;

        [NonSerialized] private object syncRoot;

        private int tail;

        public CircularBuffer(int capacity)
            : this(capacity, false)
        {
        }

        public CircularBuffer(int capacity, bool allowOverflow)
        {
            if (capacity < 0)
                throw new ArgumentException("capacity must be greater than or equal to zero.",
                    "capacity");

            this.capacity = capacity;
            Size = 0;
            head = 0;
            tail = 0;
            buffer = new T[capacity];
            AllowOverflow = allowOverflow;
        }

        public bool AllowOverflow { get; set; }

        public int Capacity
        {
            get => capacity;
            set
            {
                if (value == capacity)
                    return;

                if (value < Size)
                    throw new ArgumentOutOfRangeException("value",
                        "value must be greater than or equal to the buffer size.");

                var dst = new T[value];
                if (Size > 0)
                    CopyTo(dst);
                buffer = dst;

                capacity = value;
            }
        }

        public int Size { get; private set; }

        public bool Contains(T item)
        {
            var bufferIndex = head;
            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < Size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;

                if (item == null && buffer[bufferIndex] == null)
                    return true;
                if (buffer[bufferIndex] != null &&
                    comparer.Equals(buffer[bufferIndex], item))
                    return true;
            }

            return false;
        }

        public void Clear()
        {
            Size = 0;
            head = 0;
            tail = 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, Size);
        }

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public int Put(T[] src)
        {
            return Put(src, 0, src.Length);
        }

        public int Put(T[] src, int offset, int count)
        {
            var realCount = AllowOverflow ? count : Math.Min(count, capacity - Size);
            var srcIndex = offset;
            for (var i = 0; i < realCount; i++, tail++, srcIndex++)
            {
                if (tail == capacity)
                    tail = 0;
                buffer[tail] = src[srcIndex];
            }

            Size = Math.Min(Size + realCount, capacity);
            return realCount;
        }

        public void Put(T item)
        {
            if (!AllowOverflow && Size == capacity)
                throw new InternalBufferOverflowException("Buffer is full.");

            buffer[tail] = item;
            if (tail++ == capacity)
                tail = 0;
            Size++;
        }

        public void Skip(int count)
        {
            head += count;
            if (head >= capacity)
                head -= capacity;
        }

        public T[] Get(int count)
        {
            var dst = new T[count];
            Get(dst);
            return dst;
        }

        public int Get(T[] dst)
        {
            return Get(dst, 0, dst.Length);
        }

        public int Get(T[] dst, int offset, int count)
        {
            var realCount = Math.Min(count, Size);
            var dstIndex = offset;
            for (var i = 0; i < realCount; i++, head++, dstIndex++)
            {
                if (head == capacity)
                    head = 0;
                dst[dstIndex] = buffer[head];
            }

            Size -= realCount;
            return realCount;
        }

        public T Get()
        {
            if (Size == 0)
                throw new InvalidOperationException("Buffer is empty.");

            var item = buffer[head];
            if (head++ == capacity)
                head = 0;
            Size--;
            return item;
        }

        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (count > Size)
                throw new ArgumentOutOfRangeException("count",
                    "count cannot be greater than the buffer size.");

            var bufferIndex = head;
            for (var i = 0; i < count; i++, bufferIndex++, arrayIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;
                array[arrayIndex] = buffer[bufferIndex];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            var bufferIndex = head;
            for (var i = 0; i < Size; i++, bufferIndex++)
            {
                if (bufferIndex == capacity)
                    bufferIndex = 0;

                yield return buffer[bufferIndex];
            }
        }

        public T[] GetBuffer()
        {
            return buffer;
        }

        public T[] ToArray()
        {
            var dst = new T[Size];
            CopyTo(dst);
            return dst;
        }

        #region ICollection<T> Members

        int ICollection<T>.Count => Size;

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            Put(item);
        }

        bool ICollection<T>.Remove(T item)
        {
            if (Size == 0)
                return false;

            Get();
            return true;
        }

        #endregion

        #region ICollection Members

        int ICollection.Count => Size;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                return syncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            CopyTo((T[]) array, arrayIndex);
        }

        #endregion
    }
}