using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Caravela.Reactive.Collections;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal sealed class NamedType : Type, INamedType
    {
        internal INamedTypeSymbol NamedTypeSymbol { get; }
        internal override ITypeSymbol TypeSymbol => this.NamedTypeSymbol;

        internal NamedType(INamedTypeSymbol symbol, Compilation compilation)
            : base(compilation) => this.NamedTypeSymbol = symbol;

        public string Name => this.NamedTypeSymbol.Name;

        [Memo]
        // TODO: verify simple call to ToDisplayString gives the desired result in all cases
        public string FullName => this.NamedTypeSymbol.ToDisplayString();

        [Memo]
        public IReadOnlyList<IType> GenericArguments => this.NamedTypeSymbol.TypeArguments.Select(a => this.Compilation.SymbolMap.GetIType(a)).ToImmutableArray();

        public ITypeInfo GetTypeInfo()
        {
            return this.TypeInfo;
        }

        [Memo]
        internal TypeInfo TypeInfo => new TypeInfo(this, this.Compilation );

        [Memo]
        public ICodeElement? ContainingElement => this.TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.SymbolMap.GetTypeInfo(containingType),
            _ => throw new NotImplementedException()
        };

        [Memo]
        public IReactiveCollection<IAttribute> Attributes => this.TypeSymbol.GetAttributes().Select(a => new Attribute(a, this.Compilation.SymbolMap)).ToImmutableReactive();

        INamedType INamedType.BaseType => throw new NotImplementedException();

        IReadOnlyList<INamedType> INamedType.ImplementedInterfaces => throw new NotImplementedException();

        public override string ToString() => this.NamedTypeSymbol.ToString();
    }

    internal abstract class Type : IType
    {
        internal abstract ITypeSymbol TypeSymbol { get; }
        internal Compilation Compilation { get; }

        public IType BaseType => throw new NotImplementedException();

        public IReadOnlyList<IType> ImplementedInterfaces => throw new NotImplementedException();

        protected Type(Compilation compilation) => this.Compilation = compilation;

        public bool Is(IType other) => this.Compilation.RoslynCompilation.HasImplicitConversion( this.TypeSymbol, ((Type)other).TypeSymbol);
    }
}
