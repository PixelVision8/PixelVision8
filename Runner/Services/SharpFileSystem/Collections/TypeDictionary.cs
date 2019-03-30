using System;
using System.Collections.Generic;
using System.Collections;

namespace SharpFileSystem.Collections
{
    public class TypeDictionary<T> : ITypeDictionary<T>, IServiceProvider
    {
        #region Private Fields
        IDictionary<Type, ICollection<T>> types = new Dictionary<Type, ICollection<T>>();
        #endregion
        #region Private Helper Methods
        private ICollection<T> GetBaseCollection()
        {
            return EnsureType(GetBaseType());
        }

        private Type GetBaseType()
        {
            return typeof(T);
        }

        private void ValidateType(Type type)
        {
            if (!GetBaseType().IsAssignableFrom(type))
                throw new ArgumentException("The specified type is not a subtype of '" + GetBaseType().ToString() + "'.", "type");
        }

        private ICollection<T> AddType(Type type)
        {
            ICollection<T> result = new LinkedList<T>();
            types.Add(type, result);
            return result;
        }

        private ICollection<T> EnsureType(Type type)
        {
            ICollection<T> result;
            if (types.TryGetValue(type, out result))
                return result;
            return AddType(type);
        }

        private IEnumerable<Type> GetSubTypes(Type type)
        {
            Type currentType = type;
            while (currentType != GetBaseType().BaseType && currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
            foreach (Type interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }
        #endregion
        #region Public Transformation Methods
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            Type itemType = item.GetType();
            foreach (Type type in GetSubTypes(itemType))
            {
                ICollection<T> itemsOfType = EnsureType(type);
                if (!itemsOfType.Contains(item))
                    itemsOfType.Add(item);
            }
        }

        public bool Remove(T item)
        {
            Type itemType = item.GetType();
            foreach (Type type in GetSubTypes(itemType))
            {
                ICollection<T> itemsOfType;
                if (types.TryGetValue(type, out itemsOfType))
                {
                    if (!itemsOfType.Remove(item))
                    {
                        return false;
                    }
                    if (itemsOfType.Count == 0)
                        types.Remove(type);
                }
                else
                    return false;
            }
            return true;
        }

        public void Clear()
        {
            types.Clear();
        }
        #endregion
        #region Public Query Methods
        public IEnumerable<T> this[Type type]
        {
            get { return Get(type); }
        }

        #region Get Methods
        public IEnumerable<T> Get(Type type)
        {
            ICollection<T> itemsOfType;
            if (types.TryGetValue(type, out itemsOfType))
            {
                foreach (T item in itemsOfType)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<TGet> Get<TGet>()
        {
            ICollection<T> itemsOfType;
            if (types.TryGetValue(typeof(TGet), out itemsOfType))
            {
                foreach (object item in itemsOfType)
                {
                    yield return (TGet)item;
                }
            }
        }
        #endregion

        #region GetExplicit Methods
        public IEnumerable<T> GetExplicit(Type type)
        {
            if (type.IsAbstract)
                throw new ArgumentException("The specified type is not a instantiatable type and cannot be explicitly returned.", "type");
            ValidateType(type);
            ICollection<T> itemsOfType;
            if (types.TryGetValue(type, out itemsOfType))
            {
                foreach (T item in itemsOfType)
                {
                    if (item.GetType() == type)
                        yield return item;
                }
            }
        }

        public IEnumerable<TGet> GetExplicit<TGet>()
            where TGet : T
        {
            foreach (T item in GetExplicit(typeof(TGet)))
                yield return (TGet)item;
        }
        #endregion

        #region GetSingle Methods
        public T GetSingle(Type type)
        {
            ICollection<T> itemsOfType;
            if (types.TryGetValue(type, out itemsOfType))
            {
                foreach (T item in itemsOfType)
                {
                    return item;
                }
            }
            return default(T);
        }

        public TGet GetSingle<TGet>()
        {
            return (TGet)(object)GetSingle(typeof(TGet));
        }
        #endregion

        #region GetSingleExplicit Methods
        public T GetSingleExplicit(Type type)
        {
            if (type.IsAbstract)
                throw new ArgumentException("The specified type is not a instantiatable type and cannot be explicitly returned.", "type");
            ValidateType(type);
            ICollection<T> itemsOfType;
            if (types.TryGetValue(type, out itemsOfType))
            {
                foreach (T item in itemsOfType)
                {
                    if (item.GetType() == type)
                        return item;
                }
            }
            return default(T);
        }

        public TGet GetSingleExplicit<TGet>()
            where TGet : T
        {
            return (TGet)GetSingleExplicit(typeof(TGet));
        }
        #endregion

        #region Contains Methods
        public bool Contains(Type type)
        {
            if (type == null)
                return false;
            return types.ContainsKey(type);
        }

        public bool Contains<TContains>()
        {
            return Contains(typeof(TContains));
        }

        public bool Contains(T item)
        {
            if (item == null)
                return false;
            Type itemType = item.GetType();
            ICollection<T> itemsOfType;
            if (types.TryGetValue(itemType, out itemsOfType))
                return itemsOfType.Contains(item);
            return false;
        }
        #endregion

        public int Count
        {
            get { return GetBaseCollection().Count; }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            GetBaseCollection().CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #region GetEnumerator Methods
        public IEnumerator<T> GetEnumerator()
        {
            return GetBaseCollection().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetBaseCollection().GetEnumerator();
        }
        #endregion
        #endregion
        #region IServiceProvider Members
        object IServiceProvider.GetService(Type serviceType)
        {
            return Get(serviceType);
        }
        #endregion
    }
}
