using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using RoslynTypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SourceNamedType : NamedType
    {

        internal SourceCompilationModel Compilation { get; }

        public INamedTypeSymbol TypeSymbol { get; }

        public override string Name => this.TypeSymbol.Name;

        [Memo]
        public override string? Namespace => this.TypeSymbol.ContainingNamespace?.ToDisplayString();

        [Memo]
        public override string FullName => this.TypeSymbol.ToDisplayString();

        internal SourceNamedType(INamedTypeSymbol typeSymbol, SourceCompilationModel compilation) : base(null)
        {
            this.TypeSymbol = typeSymbol;
            this.Compilation = compilation;
        }

        public override Code.TypeKind TypeKind =>  this.TypeSymbol.TypeKind switch
        {
            RoslynTypeKind.Class => Code.TypeKind.Class,
            RoslynTypeKind.Delegate => Code.TypeKind.Delegate,
            RoslynTypeKind.Enum => Code.TypeKind.Enum,
            RoslynTypeKind.Interface => Code.TypeKind.Interface,
            RoslynTypeKind.Struct => Code.TypeKind.Struct,
            _ => throw new InvalidOperationException( $"Unexpected type kind {this.TypeSymbol.TypeKind}." )
        };

        public override IImmutableList<Member> Members => this.TypeSymbol.GetMembers()
            .Select( m => m.Kind switch {
                    SymbolKind.Event => new SourceEvent( m ),
                    SymbolKind.Property => new SourceProperty( m ),
                    SymbolKind.Method => new SourceMethod(m),
                    SymbolKind.Field => new SourceProperty( m ),
                    _ => throw new AssertionFailedException()
                    }
                );

        [Memo]
        public override IReadOnlyList<NamedType> NestedTypes => this.TypeSymbol.GetTypeMembers().Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<GenericParameter> GenericParameters =>
            this.TypeSymbol.TypeParameters.Select( tp => this.Compilation.SymbolMap.GetGenericParameter( tp ) ).ToImmutableArray();

        [Memo]
        public override IReadOnlyList<ITypeInternal> GenericArguments => this.TypeSymbol.TypeArguments.Select( a => this.Compilation.SymbolMap.GetIType( a ) ).ToImmutableList();

        [Memo]
        public override IReadOnlyList<Attribute> Attributes =>
            this.TypeSymbol.GetAttributes().Select( a => new Attribute( a, this.Compilation.SymbolMap ) ).ToImmutableList();

        [Memo]
        public override NamedType? BaseType => this.TypeSymbol.BaseType == null ? null : this.Compilation.SymbolMap.GetNamedType( this.TypeSymbol.BaseType );

        [Memo]
        public override IReadOnlyList<NamedType> ImplementedInterfaces => this.TypeSymbol.AllInterfaces.Select( this.Compilation.SymbolMap.GetNamedType ).ToImmutableList<INamedType>();

        [Memo]
        public override CodeElement? ContainingElement => this.TypeSymbol.ContainingSymbol switch
        {
            INamespaceSymbol => null,
            INamedTypeSymbol containingType => this.Compilation.SymbolMap.GetNamedType( containingType ),
            _ => throw new AssertionFailedException()
        };

        public override bool Is( IType other ) => this.Compilation.RoslynCompilation.HasImplicitConversion( this.TypeSymbol, other.GetSymbol() );

        public override bool Is( Type other ) =>
            this.Is( this.Compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public override IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreateArrayTypeSymbol( this.TypeSymbol, rank ) );

        public override IPointerType MakePointerType() =>
            (IPointerType) this.SymbolMap.GetIType( this.Compilation.RoslynCompilation.CreatePointerTypeSymbol( this.TypeSymbol ) );

        public override INamedType MakeGenericType( params IType[] genericArguments ) =>
            this.SymbolMap.GetNamedType( this.TypeSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() ) );

        public override string ToString() => this.TypeSymbol.ToString();
    }


    }
}
