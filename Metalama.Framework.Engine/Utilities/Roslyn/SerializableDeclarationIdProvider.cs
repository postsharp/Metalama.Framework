// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal static class SerializableDeclarationIdProvider
{
    public static SerializableDeclarationId GetId( ISymbol symbol ) => new( DocumentationCommentId.CreateDeclarationId( symbol ) );
}