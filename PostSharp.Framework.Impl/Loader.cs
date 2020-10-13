using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
{
    class Loader
    {
        private readonly Func<IAssemblySymbol, Assembly> loadReferencedAssembly;

        public Loader(Func<IAssemblySymbol, Assembly> loadReferencedAssembly) => this.loadReferencedAssembly = loadReferencedAssembly;

        public object CreateInstance(INamedTypeSymbol type)
        {
            var assembly = loadReferencedAssembly(type.ContainingAssembly);

            var runtimeType = assembly.GetType(type.MetadataName);

            return Activator.CreateInstance(runtimeType);
        }
    }
}
