// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class NetStandardTypeMap
    {
        public static ImmutableDictionary<string, Type> Types { get; }

        static NetStandardTypeMap()
        {
            Types = AppDomain.CurrentDomain.Load( "netstandard" )
                .GetExportedTypes()
                .ToImmutableDictionary( t => t.FullName, t => t, StringComparer.Ordinal );
        }
    }
}