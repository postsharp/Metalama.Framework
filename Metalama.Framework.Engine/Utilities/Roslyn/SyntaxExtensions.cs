﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    public static class SyntaxExtensions
    {
        internal static MemberDeclarationSyntax FindMemberDeclaration( this SyntaxNode node )
            => FindMemberDeclarationOrNull( node )
               ?? throw new AssertionFailedException( $"The {node.Kind()} at '{node.GetLocation()}' is not the descendant of a member declaration." );

        private static MemberDeclarationSyntax? FindMemberDeclarationOrNull( this SyntaxNode node )
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

            return null;
        }

        /// <summary>
        /// Find the parent node that declares an <see cref="ISymbol"/>, but not a local variable or a function.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static SyntaxNode? FindSymbolDeclaringNode( this SyntaxNode node )
        {
            var current = node;

            while ( current != null )
            {
                if ( current is MemberDeclarationSyntax or VariableDeclaratorSyntax { Parent.Parent: FieldDeclarationSyntax } )
                {
                    return current;
                }

                current = current.Parent;
            }

            return null;
        }

        internal static bool IsAutoPropertyDeclaration( this PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.ExpressionBody == null
               && propertyDeclaration.AccessorList?.Accessors.All( x => x.Body == null && x.ExpressionBody == null ) == true
               && propertyDeclaration.Modifiers.All( x => !x.IsKind( SyntaxKind.AbstractKeyword ) );

        internal static bool HasSetterAccessorDeclaration( this PropertyDeclarationSyntax propertyDeclaration )
            => propertyDeclaration.AccessorList != null
               && propertyDeclaration.AccessorList.Accessors.Any( a => a.IsKind( SyntaxKind.SetAccessorDeclaration ) );

        internal static bool IsAccessModifierKeyword( this SyntaxToken token )
            => SyntaxFacts.IsAccessibilityModifier( token.Kind() );

        internal static ExpressionSyntax RemoveParenthesis( this ExpressionSyntax node )
            => node switch
            {
                ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression.RemoveParenthesis(),
                _ => node
            };

        internal static TypeDeclarationSyntax? GetDeclaringType( this SyntaxNode node )
            => node switch
            {
                TypeDeclarationSyntax type => type,
                _ => node.Parent?.GetDeclaringType()
            };
    }
}