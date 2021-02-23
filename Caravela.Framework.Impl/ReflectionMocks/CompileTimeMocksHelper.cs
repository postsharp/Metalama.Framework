using System;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal static class CompileTimeMocksHelper
    {
        public static Exception CreateNotSupportedException() =>
            new NotSupportedException( "This object can be accessed at compile time. It can only be converted into a run-time object or converted to a ICompilation code model element using ICompilation.TypeFactory." );
    }
}