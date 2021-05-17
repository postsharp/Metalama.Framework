﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Caravela.Framework.Code.Accessibility;

namespace Caravela.Framework.Impl.Transformations
{
    public static class CodeElementExtensions
    {
        public static SyntaxTokenList GetSyntaxModifierList( this ICodeElement codeElement )
        {
            if ( codeElement is IMethod method )
            {
                // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
                var tokens = new List<SyntaxToken>();

                switch ( method.Accessibility )
                {
                    case Accessibility.Private:
                        tokens.Add( Token( SyntaxKind.PrivateKeyword ) );

                        break;

                    case Accessibility.ProtectedAndInternal:
                        tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                        break;

                    case Accessibility.Protected:
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                        break;

                    case Accessibility.Internal:
                        tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                        break;

                    case Accessibility.ProtectedOrInternal:
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                        tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                        break;

                    case Accessibility.Public:
                        tokens.Add( Token( SyntaxKind.PublicKeyword ) );

                        break;
                }

                if ( method.IsStatic )
                {
                    tokens.Add( Token( SyntaxKind.StaticKeyword ) );
                }

                if ( method.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }

                if ( method.IsVirtual )
                {
                    tokens.Add( Token( SyntaxKind.VirtualKeyword ) );
                }

                return TokenList( tokens );
            }

            throw new AssertionFailedException();
        }

        public static TypeSyntax GetSyntaxReturnType( this IMethod method )
        {
            return (TypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType.GetSymbol() );
        }

        public static TypeParameterListSyntax? GetSyntaxTypeParameterList( this IMethod method )
        {
            // TODO: generics
            return
                method.GenericParameters.Count > 0
                    ? throw new NotImplementedException()
                    : null;
        }

        public static ParameterListSyntax GetSyntaxParameterList( this IMethod method )
        {
            // TODO: generics
            return ParameterList(
                SeparatedList(
                    method.Parameters.Select(
                        p => Parameter(
                            List<AttributeListSyntax>(),
                            TokenList(), // TODO: modifiers
                            ParseTypeName( p.ParameterType.ToDisplayString() ),
                            Identifier( p.Name! ),
                            null ) ) ) );
        }

        public static SyntaxList<TypeParameterConstraintClauseSyntax> GetSyntaxConstraintClauses( this IMethod method )
        {
            // TODO: generics
            return List<TypeParameterConstraintClauseSyntax>();
        }
    }
}