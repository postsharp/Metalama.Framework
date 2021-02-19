using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    public static class CodeElementExtensions
    {
        public static SyntaxTokenList GetSyntaxModifierList( this ICodeElement codeElement )
        {
            if (codeElement is IMethod imethod)
            {
                // TODO: Unify with ToRoslynAccessibility and some roslyn helper?
                var tokens = new List<SyntaxToken>();

                switch ( imethod.Accessibility )
                {
                    case Code.Accessibility.Private:
                        tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                        break;
                    case Code.Accessibility.ProtectedAndInternal:
                        tokens.Add( Token( SyntaxKind.PrivateKeyword ) );
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                        break;
                    case Code.Accessibility.Protected:
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                        break;
                    case Code.Accessibility.Internal:
                        tokens.Add( Token( SyntaxKind.InternalKeyword ) );
                        break;
                    case Code.Accessibility.ProtectedOrInternal:
                        tokens.Add( Token( SyntaxKind.ProtectedKeyword ) );
                        tokens.Add( Token( SyntaxKind.InternalKeyword ) );
                        break;
                    case Code.Accessibility.Public:
                        tokens.Add( Token( SyntaxKind.PublicKeyword ) );
                        break;
                }

                if ( imethod.IsStatic )
                {
                    tokens.Add( Token( SyntaxKind.StaticKeyword ) );
                }

                if ( imethod.IsAbstract )
                {
                    tokens.Add( Token( SyntaxKind.AbstractKeyword ) );
                }

                if (imethod.IsVirtual)
                {
                    tokens.Add( Token( SyntaxKind.VirtualKeyword ) );
                }

                return TokenList( tokens );
            }

            throw new AssertionFailedException();
        }

        public static TypeSyntax GetSyntaxReturnType( this IMethod method )
        {
            return (TypeSyntax) CSharpSyntaxGenerator.Instance.TypeExpression( method.ReturnType.GetSymbol() );
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
                            null ) ) ));
        }

        public static SyntaxList<TypeParameterConstraintClauseSyntax> GetSyntaxConstraintClauses( this IMethod method )
        {
            // TODO: generics
            return List<TypeParameterConstraintClauseSyntax>();
        }
    }
}
