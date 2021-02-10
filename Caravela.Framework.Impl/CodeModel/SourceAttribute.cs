using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SourceAttribute : Attribute
    {
        private readonly AttributeData _data;

        private SourceCompilationModel Compilation { get; }

        public SourceAttribute( SourceCompilationModel compilation, AttributeData data )
        {
            this._data = data;
            this.Compilation = compilation;
        }

        [Memo]
        public override NamedType Type => this.Compilation.SymbolMap.GetNamedType( this._data.AttributeClass! );

        [Memo]
        public override Method Constructor => this.Compilation.SymbolMap.GetMethod( this._data.AttributeConstructor! );

        [Memo]
        public override IReadOnlyList<object?> ConstructorArguments => this._data.ConstructorArguments.Select( this.Translate ).ToImmutableArray();

        [Memo]
        public override IReadOnlyDictionary<string, object?> NamedArguments => this._data.NamedArguments.ToImmutableDictionary( kvp => kvp.Key, kvp => this.Translate( kvp.Value ) );

        private object? Translate( TypedConstant constant ) =>
            constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : this.Compilation.SymbolMap.GetIType( (ITypeSymbol) constant.Value ),
                TypedConstantKind.Array => constant.Values.Select( this.Translate ).ToImmutableArray(),
                _ => throw new ArgumentException( nameof( constant ) )
            };

        public override string ToString() => this._data.ToString();
    }
}
