using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    static class DebugEx
    {
        public static void Assert( [DoesNotReturnIf( false )] bool condition ) => Debug.Assert( condition );

        public static T AssertNotNull<T>( this T? obj ) where T : class
        {
            Assert( obj != null );
            return obj;
        }
    }
}
