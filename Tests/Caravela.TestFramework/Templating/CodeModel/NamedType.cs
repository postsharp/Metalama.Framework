using System;
using System.Collections.Immutable;
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using ITypeInternal = Caravela.Framework.Impl.CodeModel.ITypeInternal;
using SourceCompilation = Caravela.Framework.Impl.CodeModel.SourceCompilation;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class NamedType : INamedType, ITypeInternal
    {
        private readonly INamedTypeSymbol _symbol;
        private readonly SourceCompilation _compilation;

        public NamedType( INamedTypeSymbol symbol, SourceCompilation compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

        public TypeKind TypeKind => throw new NotImplementedException();

        public bool HasDefaultConstructor => throw new NotImplementedException();

        public INamedType? BaseType => throw new NotImplementedException();

        public IReactiveCollection<INamedType> ImplementedInterfaces => throw new NotImplementedException();

        public string Name => this._symbol.Name;

        public string? Namespace => throw new NotImplementedException();

        public string FullName => throw new NotImplementedException();

        public IImmutableList<IType> GenericArguments => throw new NotImplementedException();

        public IImmutableList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public bool IsOpenGeneric => throw new NotImplementedException();

        public IReactiveCollection<INamedType> NestedTypes => throw new NotImplementedException();

        public IReactiveCollection<IProperty> Properties => throw new NotImplementedException();

        public IReactiveCollection<IEvent> Events => throw new NotImplementedException();

        public IReactiveCollection<IMethod> Methods => throw new NotImplementedException();

        public ICodeElement? ContainingElement => throw new NotImplementedException();

        public IReactiveCollection<IAttribute> Attributes => throw new NotImplementedException();

        public CodeElementKind ElementKind => CodeElementKind.Type;

        public ITypeSymbol TypeSymbol => this._symbol;

        public bool Is( IType other ) => this._compilation.RoslynCompilation.HasImplicitConversion( this._symbol, other.GetSymbol() );

        public bool Is( Type other ) =>
            this.Is( this._compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public IArrayType MakeArrayType( int rank = 1 )
        {
            throw new NotImplementedException();
        }

        public IPointerType MakePointerType()
        {
            throw new NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._symbol.ToDisplayString();

        public INamedType WithGenericArguments( params IType[] genericArguments )
        {
            throw new NotImplementedException();
        }
    }
}
