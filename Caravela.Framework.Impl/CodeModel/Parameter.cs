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
        public IParameterSymbol Symbol { get; }

        private readonly CodeElement _containingMember;

        private SymbolMap SymbolMap => this._containingMember.SymbolMap;

        public Parameter( IParameterSymbol symbol, CodeElement containingMember )
        {
            this.Symbol = symbol;
            this._containingMember = containingMember;
        }

        public RefKind RefKind => this.Symbol.RefKind switch
        {
            Microsoft.CodeAnalysis.RefKind.None => RefKind.None,
            Microsoft.CodeAnalysis.RefKind.Ref => RefKind.Ref,
            Microsoft.CodeAnalysis.RefKind.Out => RefKind.Out,
            Microsoft.CodeAnalysis.RefKind.In => RefKind.In,
            _ => throw new InvalidOperationException( $"Roslyn RefKind {this.Symbol.RefKind} not recognized." )
        };

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsOut => this.RefKind == RefKind.Out;

        public bool IsParams => this.Symbol.IsParams;

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this.Symbol.Type );

        public string Name => this.Symbol.Name;

        public int Index => this.Symbol.Ordinal;

        public ICodeElement ContainingElement => this._containingMember;

        [Memo]
        public IReactiveCollection<IAttribute> Attributes => this.Symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToImmutableReactive();

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public bool HasDefaultValue => this.Symbol.HasExplicitDefaultValue;

        public object? DefaultValue => this.Symbol.ExplicitDefaultValue;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this.Symbol.ToDisplayString();
    }
}
