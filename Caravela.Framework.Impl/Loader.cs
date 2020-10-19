using System;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    class Loader
    {
        private readonly Func<IAssemblySymbol, Assembly> loadReferencedAssembly;

        public Loader(Func<IAssemblySymbol, Assembly> loadReferencedAssembly) => this.loadReferencedAssembly = loadReferencedAssembly;

        public object CreateInstance(INamedTypeSymbol type)
        {
            var assembly = this.loadReferencedAssembly(type.ContainingAssembly);

            var runtimeType = assembly.GetType(GetFullMetadataName(type));

            return Activator.CreateInstance(runtimeType);
        }

        // https://stackoverflow.com/a/27106959/41071
        private static string GetFullMetadataName(ISymbol? s)
        {
            if (s == null || IsRootNamespace(s))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(s.MetadataName);
            var last = s;

            s = s.ContainingSymbol;

            while (!IsRootNamespace(s))
            {
                if (s is ITypeSymbol && last is ITypeSymbol)
                {
                    sb.Insert(0, '+');
                }
                else
                {
                    sb.Insert(0, '.');
                }

                //sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                sb.Insert(0, s.MetadataName);
                s = s.ContainingSymbol;
            }

            return sb.ToString();
        }

        private static bool IsRootNamespace(ISymbol symbol) => symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;
    }
}
