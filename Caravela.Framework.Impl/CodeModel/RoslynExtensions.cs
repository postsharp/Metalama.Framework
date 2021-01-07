using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    static class RoslynExtensions
    {
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
