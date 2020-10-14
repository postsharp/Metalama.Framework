using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class TypeInfo : CodeElement, ITypeInfo
    {
        private readonly NamedType namedType;
        
        private INamedTypeSymbol TypeSymbol => namedType.NamedTypeSymbol;
        protected override ISymbol Symbol => TypeSymbol;

        internal override Compilation Compilation { get; }

        public TypeInfo(NamedType namedType, Compilation compilation)
        {
            this.namedType = namedType;
            Compilation = compilation;
        }

        [LazyThreadSafeProperty]
        public IReadOnlyList<ITypeInfo> NestedTypes => TypeSymbol.GetTypeMembers().Select(Cache.GetTypeInfo).ToImmutableArray();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IProperty> Properties => TypeSymbol.GetMembers().OfType<IPropertySymbol>().Select(p => new Property(p, this)).ToImmutableArray();

        public IReadOnlyList<IEvent> Events => throw new NotImplementedException();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IMethod> Methods => TypeSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => new Method(m, Compilation)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => namedType.Name;

        public string FullName => namedType.FullName;

        public IReadOnlyList<IType> GenericArguments => namedType.GenericArguments;

        [LazyThreadSafeProperty]
        public override ICodeElement? ContainingElement => TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => Cache.GetTypeInfo(containingType),
            _ => throw new NotImplementedException()
        };

        [LazyThreadSafeProperty]
        public override IReadOnlyList<IAttribute> Attributes => TypeSymbol.GetAttributes().Select(a => new Attribute(a, Cache)).ToImmutableArray();

        public ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken) => this;

        public bool Is(IType other) => namedType.Is(other);
    }
}
