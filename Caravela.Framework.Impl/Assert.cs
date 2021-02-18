using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal static class Assert
    {
        public static void True( [DoesNotReturnIf( false )] bool condition )
        {
            if ( !condition )
            {
                throw new AssertionFailedException();
            }
        }

        public static void True( [DoesNotReturnIf( false )] bool condition, string message )
        {
            if ( !condition )
            {
                throw new AssertionFailedException("Assertion failed: " + message + ".");
            }
        }

        public static T AssertNotNull<T>( this T? obj )
            where T : class
        {
            True( obj != null );
            return obj;
        }
    }
}
