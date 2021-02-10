using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{

    internal sealed class SourceProperty : Property, ISourceCodeElement
    {        
        private readonly IPropertySymbol _symbol;
        private SourceNamedType _declaringType;
        private SymbolMap SymbolMap => this._declaringType.Compilation.SymbolMap;

        public SourceCompilationModel Compilation { get; }
        public ISymbol Symbol => this._symbol;

        public override CodeElement? ContainingElement => this._declaringType;


        public SourceProperty( SourceNamedType declaringType, IPropertySymbol symbol)
        {
            this._symbol = symbol;
            this._declaringType = declaringType;
            this.Compilation = declaringType.Compilation;
        }

        public override RefKind RefKind => ReturnParameter.MapRefKind( this._symbol.RefKind );


        [Memo]
        public override ITypeInternal Type => this.Compilation.SymbolMap.GetIType( this._symbol.Type );

        [Memo]
        public override IReadOnlyList<Parameter> Parameters 
            => this._symbol.Parameters
                .Select( p => new SourceParameter( this, p ) )
                .ToImmutableArray();

        [Memo]
        public override Method? Getter => this._symbol.GetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public override Method? Setter => this._symbol.SetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.SetMethod );

        public override string Name => this._symbol.Name;

        public override bool IsStatic => this._symbol.IsStatic;

        public override bool IsVirtual => this._symbol.IsVirtual;

        public override  NamedType DeclaringType => this._declaringType;

        [Memo]
        public override IReadOnlyList<Attribute> Attributes 
            => this._symbol.GetAttributes()
                .Select( a => new SourceAttribute( this.Compilation, a ) )
                .ToImmutableArray();

        public override CodeElementKind ElementKind => CodeElementKind.Property;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) =>
            this._symbol.ToDisplayString();

        public override bool Equals( ICodeElement other ) => 
        SymbolEqualityComparer.Default.Equals( this._symbol )
    }
}
