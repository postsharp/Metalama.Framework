using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SourceField : Property, ISourceCodeElement
    {
        private readonly IFieldSymbol _symbol;

        public SourceCompilationModel Compilation { get; }

        public ISymbol Symbol => this._symbol;

        public SourceField( SourceCompilationModel compilation, IFieldSymbol symbol )
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        public override string Name => this._symbol.Name;

        public override bool IsStatic => false;

        public override bool IsVirtual => false;

        public override INamedType DeclaringType => this.Compilation.SymbolMap.GetNamedType( this._symbol.ContainingType )

        // TODO: pseudo-accessors
        [Memo]
        public override IMethod? Getter => null;

        [Memo]
        public override IMethod? Setter => null;

        public override RefKind RefKind => RefKind.None;

        [Memo]
        public override IType Type => this.Compilation.SymbolMap.GetIType( this._symbol.Type );

        public override IReadOnlyList<IParameter> Parameters => ImmutableList<IParameter>.Empty;


        [Memo]
        public override IImmutableList<Attribute> Attributes => this._symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToImmutableReactive();

        public override CodeElementKind ElementKind => CodeElementKind.Field;
    }
}
