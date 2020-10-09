using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Impl
{
    internal class TypeInfo : CodeElement, ITypeInfo
    {
        private readonly NamedType namedType;
        
        private INamedTypeSymbol Symbol => namedType.Symbol;

        internal override Compilation Compilation { get; }

        public TypeInfo(NamedType namedType, Compilation compilation)
        {
            this.namedType = namedType;
            Compilation = compilation;
        }

        [LazyThreadSafeProperty]
        public IReadOnlyList<ITypeInfo> NestedTypes => Symbol.GetTypeMembers().Select(Cache.GetTypeInfo).ToImmutableArray();

        public IReadOnlyList<IProperty> Properties => throw new NotImplementedException();

        public IReadOnlyList<IEvent> Events => throw new NotImplementedException();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IMethod> Methods => Symbol.GetMembers().OfType<IMethodSymbol>().Select(m => new Method(m, Compilation)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => namedType.Name;

        public string FullName => namedType.FullName;

        public IReadOnlyList<IType> GenericArguments => namedType.GenericArguments;

        [LazyThreadSafeProperty]
        public override ICodeElement? ContainingElement => Symbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => Cache.GetTypeInfo(containingType),
            _ => throw new NotImplementedException()
        };

        [LazyThreadSafeProperty]
        public override IReadOnlyList<IAttribute> Attributes => Symbol.GetAttributes().Select(a => new Attribute(a, Cache)).ToImmutableArray();

        public ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken) => this;
    }
}
