using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Parameter : IParameter
    {
        private readonly IParameterSymbol _symbol;

        public IParameterSymbol Symbol => this._symbol;
        private readonly CodeElement _containingMember;

        private SymbolMap SymbolMap => this._containingMember.SymbolMap;

        public Parameter(IParameterSymbol symbol, CodeElement containingMember)
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
            _ => throw new InvalidOperationException($"Roslyn RefKind {this._symbol.RefKind} not recognized.")
        };

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsOut => this.RefKind == RefKind.Out;

        public bool IsParams => this._symbol.IsParams;

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

        public string Name => this._symbol.Name;

        public int Index => this._symbol.Ordinal;

        public ICodeElement ContainingElement => this._containingMember;

        [Memo]
        public IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public CodeElementKind Kind => CodeElementKind.Parameter;

        public bool HasDefaultValue => this._symbol.HasExplicitDefaultValue;

        public object? DefaultValue => this._symbol.ExplicitDefaultValue;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null ) => this._symbol.ToDisplayString( );
    }
}
