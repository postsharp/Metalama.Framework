// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    public static class RoslynExtensions
    {
        public static bool AnyBaseType( this INamedTypeSymbol type, Predicate<INamedTypeSymbol> predicate )
        {
            for ( var t = type; t != null; t = t.BaseType )
            {
                if ( predicate( t ) )
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<INamedTypeSymbol> GetTypes( this CSharpCompilation compilation ) => compilation.GlobalNamespace.GetTypes();

        public static IEnumerable<INamedTypeSymbol> GetTypes( this IAssemblySymbol assembly ) => assembly.GlobalNamespace.GetTypes();

        private static IEnumerable<INamedTypeSymbol> GetTypes( this INamespaceSymbol ns )
        {
            foreach ( var type in ns.GetTypeMembers() )
            {
                yield return type;
            }

            foreach ( var namespaceMember in ns.GetNamespaceMembers() )
            {
                foreach ( var type in namespaceMember.GetTypes() )
                {
                    yield return type;
                }
            }
        }
    }
}
