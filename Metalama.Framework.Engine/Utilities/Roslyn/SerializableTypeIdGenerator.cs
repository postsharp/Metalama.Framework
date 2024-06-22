// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.SyntaxGeneration;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SerializableTypeIdGenerator
{
    public static SerializableTypeId GetSerializableTypeId( this ITypeSymbol symbol )
    {
        var id = SyntaxGenerationContext.Contextless.SyntaxGenerator.Type( symbol ).ToString();

        if ( symbol.NullableAnnotation != NullableAnnotation.None )
        {
            id += '!';
        }

        id = SerializableTypeIdResolverForSymbol.Prefix + id;

        return new SerializableTypeId( id );
    }

    // ReSharper disable once MemberCanBeInternal

    public static SerializableTypeId GetSerializableTypeId( this IType type, bool bypassSymbols = false )
    {
        var id = SyntaxGenerationContext.Contextless.SyntaxGenerator.Type( type, bypassSymbols ).ToString();

        if ( type.IsNullable == false && type.IsReferenceType != false )
        {
            id += '!';
        }

        id = SerializableTypeIdResolverForIType.Prefix + id;

        return new SerializableTypeId( id );
    }
}