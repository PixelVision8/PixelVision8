using System.Collections.Generic;

namespace SharpFileSystem.Collections
{
    public class InverseComparer<T> : IComparer<T>
    {
        public InverseComparer(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public IComparer<T> Comparer { get; }

        public int Compare(T x, T y)
        {
            return Comparer.Compare(y, x);
        }
    }
}