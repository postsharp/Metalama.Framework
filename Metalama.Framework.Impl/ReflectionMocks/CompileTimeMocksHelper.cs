// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal static class CompileTimeMocksHelper
    {
        // Coverage: ignore
        public static Exception CreateNotSupportedException()
            => new NotSupportedException(
                "This object can be accessed at compile time. It can only be converted into a run-time object or converted to a ICompilation code model element using ICompilation.TypeFactory." );
    }
}