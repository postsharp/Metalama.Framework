// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class SourceParameter : Parameter
    {
        private readonly ISourceCodeElement _containingMember; // Can be a type or a property.

        public SourceParameter( ISourceCodeElement containingMember, IParameterSymbol symbol )
        {
            this._containingMember = containingMember;
            this.Symbol = symbol;
        }

        public IParameterSymbol Symbol { get; }
        
        private SymbolMap SymbolMap => this._containingMember.Compilation.SymbolMap;
        
        public override bool HasDefaultValue => this.Symbol.HasExplicitDefaultValue;

        public override object? DefaultValue => this.Symbol.ExplicitDefaultValue;

        public override CodeElement? ContainingElement => (CodeElement) this._containingMember;

        public override string Name => this.Symbol.Name;

        public override  int Index => this.Symbol.Ordinal;
        
        [Memo]
        public override IType Type => this.SymbolMap.GetIType( this.Symbol.Type );

        
        public override RefKind RefKind => this.Symbol.RefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {this.Symbol.RefKind} not recognized." )
        };
        
        [Memo]
        public override  IReadOnlyList<Attribute> Attributes => this.Symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToList();


        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.Symbol.ToDisplayString();
        public override bool Equals( Parameter other ) => throw new NotImplementedException();
    }
}