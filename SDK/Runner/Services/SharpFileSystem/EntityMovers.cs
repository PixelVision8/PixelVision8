using SharpFileSystem.Collections;

namespace SharpFileSystem
{
    public static class EntityMovers
    {
        static EntityMovers()
        {
            Registration = new TypeCombinationDictionary<IEntityMover>();
        }

        public static TypeCombinationDictionary<IEntityMover> Registration { get; }
    }
}