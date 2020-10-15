using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    /// <remarks>
    /// Values of arguments are represented as:
    /// <list type="bullet">
    /// <item>Primitive types as themselves (e.g. int as int, string as string).</item>
    /// <item>Enums as their underlying type.</item>
    /// <item><see cref="System.Type"/> as <see cref="IType"/>.</item>
    /// <item>Arrays as <see cref="IReadOnlyList{T}"/>.</item>
    /// </list>
    /// </remarks>
    class Attribute : IAttribute
    {
        private readonly AttributeData data;
        private readonly SymbolMap symbolMap;

        public Attribute(AttributeData data, SymbolMap symbolMap)
        {
            this.data = data;
            this.symbolMap = symbolMap;
        }

        [Memo]
        public INamedType Type => symbolMap.GetNamedType(data.AttributeClass!);

        [Memo]
        public IReadOnlyList<object?> ConstructorArguments => data.ConstructorArguments.Select(Translate).ToImmutableArray();

        [Memo]
        public IReadOnlyDictionary<string, object?> NamedArguments => data.NamedArguments.ToImmutableDictionary(kvp => kvp.Key, kvp => Translate(kvp.Value));

        private object? Translate(TypedConstant constant) =>
            constant.Kind switch
            {
                TypedConstantKind.Primitive or TypedConstantKind.Enum => constant.Value,
                TypedConstantKind.Type => constant.Value == null ? null : symbolMap.GetIType((ITypeSymbol)constant.Value),
                TypedConstantKind.Array => constant.Values.Select(Translate).ToImmutableArray(),
                _ => throw new ArgumentException(nameof(constant))
            };
    }
}
