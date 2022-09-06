// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal static class CompileTimeMocksHelper
    {
        // Coverage: ignore
        public static Exception CreateNotSupportedException()
            => new NotSupportedException(
                "This object can be accessed at compile time. It can only be converted into a run-time object or converted to a ICompilation code model element using ICompilation.TypeFactory." );
    }
}