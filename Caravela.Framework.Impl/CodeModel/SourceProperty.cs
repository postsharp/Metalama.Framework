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

        public SourceCompilationModel Compilation { get; }
        public ISymbol Symbol => this._symbol;

        public override CodeElement? ContainingElement => this._containingElement;


        public SourceProperty( SourceCompilationModel compilation, IPropertySymbol symbol)
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        public override RefKind RefKind => ReturnParameter.MapRefKind( this._symbol.RefKind );


        [Memo]
        public override ITypeInternal Type => this.Compilation.SymbolMap.GetIType( this._symbol.Type );

        [Memo]
        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableList<IParameter>();

        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.SetMethod );

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => this._symbol.IsVirtual;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IReadOnlyList<Attribute> Attributes => this._symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToImmutableReactive();

        public override CodeElementKind ElementKind => CodeElementKind.Property;
    }
}
