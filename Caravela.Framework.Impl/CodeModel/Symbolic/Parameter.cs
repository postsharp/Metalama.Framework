using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class Parameter : CodeElement, IParameter
    {
        public IParameterSymbol ParameterSymbol { get; }

        private readonly CodeElement _containingMember;

        public Parameter( IParameterSymbol symbol, CodeElement containingMember ) : base( containingMember.Compilation )
        {
            this.ParameterSymbol = symbol;
            this._containingMember = containingMember;
        }

        public RefKind RefKind => this.ParameterSymbol.RefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {this.ParameterSymbol.RefKind} not recognized." )
        };

        [Memo]
        public IType Type => this.Compilation.GetIType( this.ParameterSymbol.Type );

        public string Name => this.ParameterSymbol.Name;

        public int Index => this.ParameterSymbol.Ordinal;

        public bool IsParams => this.ParameterSymbol.IsParams;

        public override ICodeElement ContainingElement => this._containingMember;

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        protected internal override ISymbol Symbol => this.ParameterSymbol;

        public OptionalValue DefaultValue => this.ParameterSymbol.HasExplicitDefaultValue ? new OptionalValue( this.ParameterSymbol.ExplicitDefaultValue ) : default;
    }
}
