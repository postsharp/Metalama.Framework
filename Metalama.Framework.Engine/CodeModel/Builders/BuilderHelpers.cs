// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Metalama.Framework.Code.Accessibility;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal static class BuilderHelpers
    {
        public static void AddTokens( this Accessibility accessibility, List<SyntaxToken> tokenList )
        {
            switch ( accessibility )
            {
                case Accessibility.Private:
                    tokenList.Add( Token( SyntaxKind.PrivateKeyword ) );

                    break;

                case Accessibility.Protected:
                    tokenList.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Internal:
                    tokenList.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.PrivateProtected:
                    tokenList.Add( Token( SyntaxKind.PrivateKeyword ) );
                    tokenList.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.ProtectedInternal:
                    tokenList.Add( Token( SyntaxKind.ProtectedKeyword ) );
                    tokenList.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.Public:
                    tokenList.Add( Token( SyntaxKind.PublicKeyword ) );

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        public static void AddReturnValueTokens( this RefKind refKind, List<SyntaxToken> tokenList )
        {
            switch ( refKind )
            {
                case RefKind.Ref:
                    tokenList.Add( Token( SyntaxKind.RefKeyword ) );

                    break;

                case RefKind.RefReadOnly:
                    tokenList.Add( Token( SyntaxKind.RefKeyword ) );
                    tokenList.Add( Token( SyntaxKind.ReadOnlyKeyword ) );

                    break;

                case RefKind.Out:
                    tokenList.Add( Token( SyntaxKind.OutKeyword ) );

                    break;

                case RefKind.None:
                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        public static void AddParameterTokens( this RefKind refKind, List<SyntaxToken> tokenList )
        {
            switch ( refKind )
            {
                case RefKind.Ref:
                    tokenList.Add( Token( SyntaxKind.RefKeyword ) );

                    break;

                case RefKind.In:
                    tokenList.Add( Token( SyntaxKind.InKeyword ) );

                    break;

                case RefKind.Out:
                    tokenList.Add( Token( SyntaxKind.OutKeyword ) );

                    break;

                case RefKind.None:
                    break;

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}