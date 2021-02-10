using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SourceNamedType : NamedType, ISourceCodeElement
    {
        private readonly INamedTypeSymbol _symbol;

        public SourceCompilationModel Compilation { get; }

        internal SourceNamedType( SourceCompilationModel compilation, INamedTypeSymbol symbol )
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        ISymbol ISourceCodeElement.Symbol => this._symbol;

        public INamedTypeSymbol Symbol => this._symbol;

        public override string Name => this._symbol.Name;

        [Memo]
        public override string? Namespace => this._symbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        public override string FullName => this._symbol.ToDisplayString();


        public override Code.TypeKind TypeKind => this._symbol.TypeKind switch
        {
            RoslynTypeKind.Class => Code.TypeKind.Class,
            RoslynTypeKind.Delegate => Code.TypeKind.Delegate,
            RoslynTypeKind.Enum => Code.TypeKind.Enum,
            RoslynTypeKind.Interface => Code.TypeKind.Interface,
            RoslynTypeKind.Struct => Code.TypeKind.Struct,
            _ => throw new InvalidOperationException( $"Unexpected type kind {this._symbol.TypeKind}." )
        };

        public override IReadOnlyList<Member> Members => this._symbol.GetMembers()
            .Select( m => m.Kind switch {
                    SymbolKind.Event => new SourceEvent( this.Compilation, (IEventSymbol)m ),
                    SymbolKind.Property => new SourceProperty( this.Compilation, (IPropertySymbol)m ),
                    SymbolKind.Method => new SourceMethod( this.Compilation, (IMethodSymbol)m ),
                    SymbolKind.Field => new SourceField( this.Compilation, (IFieldSymbol)m ),
                    _ => throw new AssertionFailedException()
                    }
                );

        [Memo]
        public override IReadOnlyList<NamedType> NestedTypes => this._symbol.GetTypeMembers().Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<GenericParameter> GenericParameters =>
            this._symbol.TypeParameters.Select( tp => this.Compilation.SymbolMap.GetGenericParameter( tp ) ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<ITypeInternal> GenericArguments => this._symbol.TypeArguments.Select( a => this.Compilation.SymbolMap.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override IReadOnlyList<Attribute> Attributes =>
            this._symbol.GetAttributes().Select( a => new SourceAttribute( this.Compilation, a ) ).ToImmutableList();

        [Memo]
        public override NamedType? BaseType => this._symbol.BaseType == null ? null : this.Compilation.SymbolMap.GetNamedType( this._symbol.BaseType );

        [Memo]
        public override IReadOnlyList<NamedType> ImplementedInterfaces => this._symbol.AllInterfaces.Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableList();

        [Memo]
        public override CodeElement? ContainingElement => this._symbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.SymbolMap.GetNamedType( containingType ),
            _ => throw new AssertionFailedException()
        };

        public override bool Is( IType other ) => this.Compilation.RoslynCompilation.HasImplicitConversion( this._symbol, other.GetSymbol() );

        public override bool Is( Type other ) =>
            this.Is( this.Compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public override IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this.Compilation.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreateArrayTypeSymbol( this._symbol, rank ) );

        public override IPointerType MakePointerType() =>
            (IPointerType) this.Compilation.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreatePointerTypeSymbol( this._symbol ) );

        public override INamedType MakeGenericType( params IType[] genericArguments ) =>
            this.Compilation.SymbolMap.GetNamedType( this._symbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public override string ToString() => this._symbol.ToString();    
    }
}
