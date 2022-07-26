// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking
{
    internal static class SymbolExtensions
    {
        public static LinkerDeclarationFlags GetDeclarationFlags( this ISymbol symbol )
        {
            // TODO: Partials?
            var declaration = symbol.GetPrimaryDeclaration();

            switch ( declaration )
            {
                case MemberDeclarationSyntax memberDeclaration:
                    return memberDeclaration.GetLinkerDeclarationFlags();

                case VariableDeclaratorSyntax variableDeclarator:
                    return ((MemberDeclarationSyntax?) variableDeclarator.Parent?.Parent).AssertNotNull().GetLinkerDeclarationFlags();

                case ParameterSyntax { Parent: { Parent: RecordDeclarationSyntax } }:
                    return default;

                case null:
                    return default;

                default:
                    throw new AssertionFailedException();
            }
        }

        public static bool IsExplicitInterfaceEventField( this ISymbol symbol )
        {
            if ( symbol is IEventSymbol eventSymbol )
            {
                var declaration = eventSymbol.GetPrimaryDeclaration();

                if ( declaration != null && declaration.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.EventField ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}