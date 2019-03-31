using SharpFileSystem.Collections;

namespace SharpFileSystem
{
    public static class EntityCopiers
    {
        static EntityCopiers()
        {
            Registration = new TypeCombinationDictionary<IEntityCopier>();
        }

        public static TypeCombinationDictionary<IEntityCopier> Registration { get; }
    }
}