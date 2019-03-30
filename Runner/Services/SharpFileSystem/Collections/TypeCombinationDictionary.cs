using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpFileSystem.Collections
{
    public class TypeCombinationDictionary<T>
    {
        public class TypeCombinationEntry
        {
            public Type SourceType { get; private set; }
            public Type DestinationType { get; private set; }
            public T Value { get; private set; }

            public TypeCombinationEntry(Type sourceType, Type destinationType, T value)
            {
                SourceType = sourceType;
                DestinationType = destinationType;
                Value = value;
            }
        }

        private LinkedList<TypeCombinationEntry> _registrations = new LinkedList<TypeCombinationEntry>();

        public IEnumerable<TypeCombinationEntry> GetSupportedRegistrations(Type sourceType, Type destinationType)
        {
            return
                _registrations.Where(
                    r =>
                    r.SourceType.IsAssignableFrom(sourceType) && r.DestinationType.IsAssignableFrom(destinationType));
        }

        public TypeCombinationEntry GetSupportedRegistration(Type sourceType, Type destinationType)
        {
            return GetSupportedRegistrations(sourceType, destinationType).FirstOrDefault();
        }

        public bool TryGetSupported(Type sourceType, Type destinationType, out T value)
        {
            var r = GetSupportedRegistration(sourceType, destinationType);
            if (r == null)
            {
                value = default(T);
                return false;
            }
            value = r.Value;
            return true;
        }

        public void AddFirst(Type sourceType, Type destinationType, T value)
        {
            _registrations.AddFirst(new TypeCombinationEntry(sourceType, destinationType, value));
        }

        public void AddLast(Type sourceType, Type destinationType, T value)
        {
            _registrations.AddLast(new TypeCombinationEntry(sourceType, destinationType, value));
        }

    }
}
