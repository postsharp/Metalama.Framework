using System;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal static class ReflectionSerializationExceptionHelper
    {
        public static Exception CreateNotSupportedException() =>
            new NotSupportedException( "This object can be accessed at compile time. It can only be converted into a run-time object." );
    }
}