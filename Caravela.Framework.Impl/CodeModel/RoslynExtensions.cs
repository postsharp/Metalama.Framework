using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    static class RoslynExtensions
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

        public static IEnumerable<INamedTypeSymbol> GetTypes(this IAssemblySymbol assembly) => assembly.GlobalNamespace.GetTypes();

        private static IEnumerable<INamedTypeSymbol> GetTypes(this INamespaceSymbol ns)
        {
            foreach (var type in ns.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var namespaceMember in ns.GetNamespaceMembers())
            {
                foreach (var type in namespaceMember.GetTypes())
                {
                    yield return type;
                }
            }
        }
    }
}
