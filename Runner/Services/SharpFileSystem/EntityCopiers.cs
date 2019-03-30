using SharpFileSystem.Collections;

namespace SharpFileSystem
{
    public static class EntityCopiers
    {
        public static TypeCombinationDictionary<IEntityCopier> Registration { get; private set; }

        static EntityCopiers()
        {
            Registration = new TypeCombinationDictionary<IEntityCopier>();
        }
    }
}
