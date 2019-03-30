using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpFileSystem
{
    public static class Extensions
    {
        public static void Add<TKey, TValue>(ICollection<KeyValuePair<TKey, TValue>> collection, TKey key, TValue value)
        {
            collection.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }
}
