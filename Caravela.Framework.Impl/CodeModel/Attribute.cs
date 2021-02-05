using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Attribute : IAttribute
    {
        private readonly AttributeData _data;
        private readonly SymbolMap _symbolMap;

        public Attribute( AttributeData data, SymbolMap symbolMap )
        {
            this._data = data;
            this._symbolMap = symbolMap;
        }

        [Memo]
        public INamedType Type => this._symbolMap.GetNamedType( this._data.AttributeClass! );

        [Memo]
        public IMethod Constructor => this._symbolMap.GetMethod( this._data.AttributeConstructor! );

        [Memo]
        public IImmutableList<object?> ConstructorArguments => this._data.ConstructorArguments.Select( this.Translate ).ToImmutableList();

        [Memo]
        public IReadOnlyDictionary<string, object?> NamedArguments => this._data.NamedArguments.ToImmutableDictionary( kvp => kvp.Key, kvp => this.Translate( kvp.Value ) );

        private object? Translate( TypedConstant constant ) =>
            constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : this._symbolMap.GetIType( (ITypeSymbol) constant.Value ),
                TypedConstantKind.Array => constant.Values.Select( this.Translate ).ToImmutableArray(),
                _ => throw new ArgumentException( nameof( constant ) )
            };

        public override string ToString() => this._data.ToString();
    }
}
