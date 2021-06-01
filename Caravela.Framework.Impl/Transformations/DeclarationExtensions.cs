// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    public static class DeclarationExtensions
    {
        public static SyntaxTokenList GetSyntaxModifierList( this IDeclaration declaration )
        {
            switch ( declaration )
            {
                case IMethod accessor when accessor.IsAccessor():
                    return GetAccessorSyntaxModifierList( accessor );

                case IMethod method:
                    return GetMemberSyntaxModifierList( method );

                case IProperty property:
                    return GetMemberSyntaxModifierList( property );
            }

            throw new AssertionFailedException();
        }

        private static SyntaxTokenList GetAccessorSyntaxModifierList( IMethod accessor )
        {
            var methodGroup = (IMemberOrNamedType) accessor.ContainingDeclaration!;

            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            if ( accessor.Accessibility != methodGroup.Accessibility )
            {
                AddAccessibilityTokens( accessor, tokens );
            }

            return TokenList( tokens );
        }

        private static SyntaxTokenList GetMemberSyntaxModifierList( IMember member )
        {
            // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
            var tokens = new List<SyntaxToken>();

            AddAccessibilityTokens( member, tokens );

            if ( member.IsStatic )
            {
                tokens.Add( Token( SyntaxKind.StaticKeyword ) );
            }

            if ( member.IsAbstract )
            {
                tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
            }

            if ( member.IsVirtual )
            {
                tokens.Add( Token( SyntaxKind.VirtualKeyword ) );
            }

            return TokenList( tokens );
        }

        private static void AddAccessibilityTokens( IMemberOrNamedType member, List<SyntaxToken> tokens )
        {
            switch ( member.Accessibility )
            {
                case Accessibility.Private:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );

                    break;

                case Accessibility.PrivateProtected:
                    tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Protected:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );

                    break;

                case Accessibility.Internal:
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.ProtectedInternal:
                    tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                    tokens.Add( Token( SyntaxKind.InternalKeyword ) );

                    break;

                case Accessibility.Public:
                    tokens.Add( Token( SyntaxKind.PublicKeyword ) );

                    break;
            }
        }

        public static NameSyntax GetSyntaxTypeName( this IType type )
        {
            return (NameSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( type.GetSymbol() );
        }

        public static TypeSyntax GetSyntaxReturnType( this IMethod method )
        {
            return (TypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.ReturnType.GetSymbol() );
        }

        public static TypeSyntax GetSyntaxReturnType( this IProperty method )
        {
            return (TypeSyntax) LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( method.Type.GetSymbol() );
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