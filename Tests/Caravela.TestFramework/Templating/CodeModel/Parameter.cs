using System;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;
using SourceCompilation = Caravela.Framework.Impl.CodeModel.SourceCompilation;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class Parameter : IParameter
    {
        private readonly IParameterSymbol _symbol;
        private readonly SourceCompilation _compilation;

        public Parameter( IParameterSymbol symbol, SourceCompilation compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

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

        public IType Type => new NamedType( (INamedTypeSymbol) this._symbol.Type, this._compilation );

        public string? Name => this._symbol.Name;

        public int Index => this._symbol.Ordinal;

        public bool HasDefaultValue => this._symbol.HasExplicitDefaultValue;

        public object? DefaultValue => this._symbol.ExplicitDefaultValue;

        public ICodeElement? ContainingElement => throw new NotImplementedException();

        public IReactiveCollection<IAttribute> Attributes => throw new NotImplementedException();

        public CodeElementKind ElementKind => CodeElementKind.Parameter;

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._symbol.ToDisplayString();
    }
}
