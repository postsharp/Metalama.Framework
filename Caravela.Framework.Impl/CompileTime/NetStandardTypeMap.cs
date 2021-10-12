// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class NetStandardTypeMap
    {
        private static ImmutableHashSet<Type> Types { get; }

        static NetStandardTypeMap()
        {
            AppDomain.CurrentDomain.Load( "netstandard" ).GetExportedTypes().ToImmutableHashSet();
        }
    }
}