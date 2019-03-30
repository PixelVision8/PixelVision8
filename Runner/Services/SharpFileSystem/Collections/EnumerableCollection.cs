using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpFileSystem.Collections
{
    public class EnumerableCollection<T>: ICollection<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public EnumerableCollection(IEnumerable<T> enumerable, int count)
        {
            _enumerable = enumerable;
            Count = count;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _enumerable.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return this.Any(v => item.Equals(v));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array.Length < Count + arrayIndex)
                throw new ArgumentOutOfRangeException(nameof(array), "The supplied array (of size " + array.Length + ") cannot contain " + Count + " items on index " + arrayIndex);
            foreach (var item in _enumerable)
            {
                array[arrayIndex++] = item;
            }
        }

        #region Unsupported methods
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
