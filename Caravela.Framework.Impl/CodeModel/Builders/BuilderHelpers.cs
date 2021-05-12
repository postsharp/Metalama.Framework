// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Caravela.Framework.Code.Accessibility;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal static class BuilderHelpers
    {
        public static void GetTokens( this Accessibility accessibility, List<SyntaxToken> tokenList)
        {
            switch ( accessibility )
            {
                case Accessibility.Private:
                    tokenList.Add( Token( SyntaxKind.PrivateKeyword ) );
                    break;
                case Accessibility.Protected:
                    tokenList.Add( Token( SyntaxKind.ProtectedKeyword) );
                    break;
                case Accessibility.Internal:
                    tokenList.Add( Token( SyntaxKind.InternalKeyword ) );
                    break;
                case Accessibility.ProtectedAndInternal:
                    tokenList.Add( Token( SyntaxKind.PrivateKeyword ) );
                    tokenList.Add( Token( SyntaxKind.InternalKeyword ) );
                    break;
                case Accessibility.ProtectedOrInternal:
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

        public static void GetReturnValueTokens( this RefKind refKind, List<SyntaxToken> tokenList )
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

        public static void GetParameterTokens( this RefKind refKind, List<SyntaxToken> tokenList )
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
