using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Parameter : CodeElement, IParameter
    {
        private readonly IParameterSymbol _symbol;


        private readonly CodeElement _containingMember;

        public Parameter( IParameterSymbol symbol, CodeElement containingMember ) : base( containingMember.Compilation )
        {
            this._symbol = symbol;
            this._containingMember = containingMember;
        }

        public RefKind RefKind => this._symbol.RefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {this._symbol.RefKind} not recognized." )
        };

        [Memo]
        public IType Type => this.Compilation.GetIType( this._symbol.Type );

        public string Name => this._symbol.Name;

        public int Index => this._symbol.Ordinal;

        public override ICodeElement ContainingElement => this._containingMember;

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        protected internal override ISymbol Symbol => this._symbol;

    
        public OptionalValue DefaultValue => this._symbol.HasExplicitDefaultValue ? new OptionalValue(this._symbol.ExplicitDefaultValue) : default;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._symbol.ToDisplayString();
    }
}
