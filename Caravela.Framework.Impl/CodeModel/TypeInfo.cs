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

        [Memo]
        public IReadOnlyList<ITypeInfo> NestedTypes => TypeSymbol.GetTypeMembers().Select(Cache.GetTypeInfo).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IProperty> Properties => TypeSymbol.GetMembers().OfType<IPropertySymbol>().Select(p => new Property(p, this)).ToImmutableArray();

        public IReadOnlyList<IEvent> Events => throw new NotImplementedException();

        [Memo]
        public IReadOnlyList<IMethod> Methods => TypeSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => new Method(m, Compilation)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => namedType.Name;

        public string FullName => namedType.FullName;

        public IReadOnlyList<IType> GenericArguments => namedType.GenericArguments;

        public override ICodeElement? ContainingElement => namedType.ContainingElement;

        public override IReadOnlyList<IAttribute> Attributes => namedType.Attributes;

        public ITypeInfo GetTypeInfo(ITypeResolutionToken typeResolutionToken) => this;

        public bool Is(IType other) => namedType.Is(other);
    }
}
