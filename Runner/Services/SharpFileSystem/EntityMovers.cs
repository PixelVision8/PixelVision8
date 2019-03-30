using System.Text;
using SharpFileSystem.Collections;

namespace SharpFileSystem
{
    public static class EntityMovers
    {
        public static TypeCombinationDictionary<IEntityMover> Registration { get; private set; }

        static EntityMovers()
        {
            Registration = new TypeCombinationDictionary<IEntityMover>();
        }
    }
}
