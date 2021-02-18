using System;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using CompilationModel = Caravela.Framework.Impl.CodeModel.Symbolic.CompilationModel;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class Parameter : CodeElement, IParameter
    {
        private readonly IParameterSymbol _symbol;

        public Parameter( IParameterSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        protected internal override ISymbol Symbol => this._symbol;

        public RefKind RefKind => this._symbol.RefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {this._symbol.RefKind} not recognized." )
        };

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsOut => this.RefKind == RefKind.Out;

        public bool IsParams => this._symbol.IsParams;

        public IType ParameterType => new NamedType( (INamedTypeSymbol) this._symbol.Type, this.Compilation );

        public string Name => this._symbol.Name;

        public int Index => this._symbol.Ordinal;

        public bool HasDefaultValue => this._symbol.HasExplicitDefaultValue;

        public OptionalValue DefaultValue => this._symbol.HasExplicitDefaultValue ? new OptionalValue( this._symbol.ExplicitDefaultValue ) : default;

        public override CodeElementKind ElementKind => CodeElementKind.Parameter;

        public IMember DeclaringMember => throw new NotImplementedException();
    }
}
