// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Metalama.Framework.Code.Accessibility;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal static class BuilderHelpers
{
    public static void AddTokens( this Accessibility accessibility, List<SyntaxToken> tokenList )
    {
        switch ( accessibility )
        {
            case Accessibility.Private:
                tokenList.Add( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );

                break;

            case Accessibility.Protected:
                tokenList.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );

                break;

            case Accessibility.Internal:
                tokenList.Add( Token( SyntaxKind.InternalKeyword ).WithTrailingTrivia( Space ) );

                break;

            case Accessibility.PrivateProtected:
                tokenList.Add( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( Space ) );
                tokenList.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );

                break;

            case Accessibility.ProtectedInternal:
                tokenList.Add( Token( SyntaxKind.ProtectedKeyword ).WithTrailingTrivia( Space ) );
                tokenList.Add( Token( SyntaxKind.InternalKeyword ).WithTrailingTrivia( Space ) );

                break;

            case Accessibility.Public:
                tokenList.Add( Token( SyntaxKind.PublicKeyword ).WithTrailingTrivia( Space ) );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected Accessibility: {accessibility}." );
        }
    }

    /*
    public static void AddReturnValueTokens( this RefKind refKind, List<SyntaxToken> tokenList )
    {
        switch ( refKind )
        {
            case RefKind.Ref:
                tokenList.Add( Token( SyntaxKind.RefKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.RefReadOnly:
                tokenList.Add( Token( SyntaxKind.RefKeyword ).WithTrailingTrivia( Space ) );
                tokenList.Add( Token( SyntaxKind.ReadOnlyKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.Out:
                tokenList.Add( Token( SyntaxKind.OutKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.None:
                break;

            default:
                throw new AssertionFailedException( $"Unexpected RefKind: {refKind}." );
        }
    }

    public static void AddParameterTokens( this RefKind refKind, List<SyntaxToken> tokenList )
    {
        switch ( refKind )
        {
            case RefKind.Ref:
                tokenList.Add( Token( SyntaxKind.RefKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.In:
                tokenList.Add( Token( SyntaxKind.InKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.Out:
                tokenList.Add( Token( SyntaxKind.OutKeyword ).WithTrailingTrivia( Space ) );

                break;

            case RefKind.None:
                break;

            default:
                throw new AssertionFailedException( $"{refKind} is not supported." );
        }
    }
    */
}