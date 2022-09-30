// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.ReflectionMocks
{
    internal static class CompileTimeMocksHelper
    {
        // Coverage: ignore
        public static Exception CreateNotSupportedException(string typeName)
            => new NotSupportedException(
                $"This instance of {typeName} cannot be accessed at compile time because it represents a run-time object. Try using meta.RunTime() to convert this object to its run-time value." );
    }
}