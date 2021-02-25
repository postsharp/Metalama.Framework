using System.Collections.Generic;

namespace Caravela.Framework.UnitTests.Templating.Serialization
{

    public class CustomComparer<T> : IEqualityComparer<T>
    {
        public bool Equals( T? x, T? y ) => true;

        public int GetHashCode( T obj ) => 0;
    }
}