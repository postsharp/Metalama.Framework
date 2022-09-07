// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    public static class SyntaxExtensions
    {
        public static MemberDeclarationSyntax FindMemberDeclaration( this SyntaxNode? node )
        {
            var current = node;

            while ( current != null )
            {
                if ( current is MemberDeclarationSyntax memberDeclaration )
                {
                    return memberDeclaration;
                }

                current = current.Parent;
            }

            throw new AssertionFailedException();
        }

        public static bool IsAutoPropertyDeclaration( this PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.ExpressionBody == null
               && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
               && propertyDeclaration.Modifiers.All( x => !x.IsKind( SyntaxKind.AbstractKeyword ) );

        public static bool HasSetterAccessorDeclaration( this PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.AccessorList != null
               && propertyDeclaration.AccessorList.Accessors.Any( a => a.IsKind( SyntaxKind.SetAccessorDeclaration ) );

        public static bool IsAccessModifierKeyword( this SyntaxToken token )
            => token.Kind() switch
            {
                SyntaxKind.PrivateKeyword => true,
                SyntaxKind.ProtectedKeyword => true,
                SyntaxKind.InternalKeyword => true,
                SyntaxKind.PublicKeyword => true,
                _ => false
            };

        public static ExpressionSyntax RemoveParenthesis( this ExpressionSyntax node )
            => node switch
            {
                ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression.RemoveParenthesis(),
                _ => node
            };
    }
}