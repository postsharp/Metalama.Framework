// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

public static class SerializableDeclarationIdProvider
{
    public static SerializableDeclarationId GetSerializableId( this ISymbol symbol )
    {
        var id = DocumentationCommentId.CreateDeclarationId( symbol );

        if ( id == null )
        {
            throw new ArgumentOutOfRangeException( $"Cannot create a {nameof(SerializableDeclarationId)} for '{symbol}'." );
        }

        return new SerializableDeclarationId( id );
    }

    public static ISymbol? Resolve( this SerializableDeclarationId id, Compilation compilation )
        => DocumentationCommentId.GetFirstSymbolForDeclarationId( id.ToString(), compilation );
}