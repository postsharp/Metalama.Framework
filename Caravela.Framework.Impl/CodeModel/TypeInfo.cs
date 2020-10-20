using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal class TypeInfo : CodeElement, ITypeInfo
    {
        private readonly NamedType _namedType;
        
        internal INamedTypeSymbol TypeSymbol => this._namedType.NamedTypeSymbol;
        protected override ISymbol Symbol => this.TypeSymbol;

        internal override Compilation Compilation { get; }

        public TypeInfo(NamedType namedType, Compilation compilation)
        {
            this._namedType = namedType;
            this.Compilation = compilation;
        }

        [Memo]
        public IReadOnlyList<ITypeInfo> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.SymbolMap.GetTypeInfo).ToImmutableArray();

        [Memo]
        public IReadOnlyList<IProperty> Properties => this.TypeSymbol.GetMembers().OfType<IPropertySymbol>().Select(p => new Property(p, this)).ToImmutableArray();

        public IReadOnlyList<IEvent> Events => throw new NotImplementedException();

        [Memo]
        public IReadOnlyList<IMethod> Methods => this.TypeSymbol.GetMembers().OfType<IMethodSymbol>().Select(m => this.SymbolMap.GetMethod(m)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => this._namedType.Name;

        public string FullName => this._namedType.FullName;

        public IReadOnlyList<IType> GenericArguments => this._namedType.GenericArguments;

        public override ICodeElement? ContainingElement => this._namedType.ContainingElement;

        public override IReactiveCollection<IAttribute> Attributes => this._namedType.Attributes;

        public IType BaseType => throw new NotImplementedException();

        public IReadOnlyList<IType> ImplementedInterfaces => throw new NotImplementedException();

        INamedType INamedType.BaseType => throw new NotImplementedException();

        IReadOnlyList<INamedType> INamedType.ImplementedInterfaces => throw new NotImplementedException();

        public ITypeInfo GetTypeInfo(in ReactiveObserverToken observerToken) => this;

        public bool Is(IType other) => this._namedType.Is(other);

        public ITypeInfo GetTypeInfo()
        {
            throw new NotImplementedException();
        }
    }
}
