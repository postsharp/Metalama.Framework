// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

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

    internal static bool IsNameOf( this InvocationExpressionSyntax node )
        => node.Expression.Kind() == SyntaxKind.NameOfKeyword ||
           (node.Expression is IdentifierNameSyntax identifierName && string.Equals( identifierName.Identifier.Text, "nameof", StringComparison.Ordinal ));

    internal static TypeSyntax GetNamespaceOrType( this UsingDirectiveSyntax usingDirective )
#if ROSLYN_4_8_0_OR_GREATER
        => usingDirective.NamespaceOrType;
#else
        => usingDirective.Name;
#endif

    internal static ParameterListSyntax? GetParameterList( this TypeDeclarationSyntax typeDeclaration )
    {
#if ROSLYN_4_8_0_OR_GREATER
        return typeDeclaration.ParameterList;
#else
        return typeDeclaration switch
        {
            RecordDeclarationSyntax record => record.ParameterList,
            _ => null
        };
#endif
    }

#if !ROSLYN_4_8_0_OR_GREATER
    internal static TypeDeclarationSyntax WithParameterList( this TypeDeclarationSyntax typeDeclaration, ParameterListSyntax? parameterList )
        => typeDeclaration is RecordDeclarationSyntax record ? record.WithParameterList( parameterList ) :
            parameterList == null ? typeDeclaration :
            throw new InvalidOperationException( $"Can't add parameter list to a non-record type before C# 12." );
#endif

    internal static TNode NormalizeWhitespaceIfNecessary<TNode>( this TNode node, bool normalizeWhitespace )
        where TNode : SyntaxNode
    {
        if ( !normalizeWhitespace )
        {
            return node;
        }

#pragma warning disable LAMA0830 // NormalizeWhitespace is expensive.
        return node.NormalizeWhitespace( elasticTrivia: true );
#pragma warning restore LAMA0830
    }
}