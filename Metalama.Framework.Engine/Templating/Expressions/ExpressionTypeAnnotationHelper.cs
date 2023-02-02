// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using System.Linq;

namespace Metalama.Framework.Engine.Templating.Expressions;

/// <summary>
/// Annotates an <see cref="ExpressionSyntax"/> with an annotation that specifies its type.
/// </summary>
internal static class ExpressionTypeAnnotationHelper
{
    private const string _typeAnnotationKind = "metalama-typeid";

    public static ExpressionSyntax WithTypeAnnotation( this ExpressionSyntax node, ITypeSymbol? type, Compilation? compilation )
    {
        if ( type != null && compilation != null && !node.GetAnnotations( _typeAnnotationKind ).Any() )
        {
            var syntaxAnnotation = SymbolAnnotationMapper.GetOrCreateAnnotation( 
                _typeAnnotationKind,
                type );

            ExpressionSyntax AddAnnotationRecursive( ExpressionSyntax n )
            {
                if ( n is ParenthesizedExpressionSyntax parenthesizedExpression )
                {
                    return parenthesizedExpression.WithExpression( AddAnnotationRecursive( parenthesizedExpression.Expression ) )
                        .WithAdditionalAnnotations( syntaxAnnotation );
                }
                else
                {
                    return n.WithAdditionalAnnotations( syntaxAnnotation );
                }
            }

            return AddAnnotationRecursive( node );
        }
        else
        {
            return node;
        }
    }

    public static bool TryFindTypeFromAnnotation( SyntaxNode node, Compilation compilation, out ITypeSymbol? type )
    {
        // If we don't know the exact type, check if we have a type annotation on the syntax.

        var typeAnnotation = node.GetAnnotations( _typeAnnotationKind ).FirstOrDefault();

        if ( typeAnnotation != null! )
        {
            type = (ITypeSymbol) SymbolAnnotationMapper.GetSymbolFromAnnotation( typeAnnotation );
        }
        else if ( SyntaxTreeAnnotationMap.TryGetExpressionType( node, out var symbol ) )
        {
            type = (ITypeSymbol) symbol;
        }
        else
        {
            type = null;

            return false;
        }

        type = (ITypeSymbol)type.Translate( null, compilation );

        return true;
    }
}