using System.Collections.Generic;

namespace SharpFileSystem.Collections
{
    public class InverseComparer<T>: IComparer<T>
    {
        public IComparer<T> Comparer { get; private set; }
        public InverseComparer(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            return Comparer.Compare(y, x);
        }
    }
}
