// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Templating
{
    // This class uses WeakReference<ISymbol> because it seems that Roslyn's ObjectPool may cache SyntaxAnnotation beyond the lifetime of a compilation.

    internal static class SymbolAnnotationMapper
    {
        public const string ExpressionTypeAnnotationKind = "Metalama.ExpressionType";

        private static readonly ConditionalWeakTable<SyntaxAnnotation, WeakReference<ISymbol>> _annotationToSymbolMap = new();
        private static readonly ConditionalWeakTable<ISymbol, List<SyntaxAnnotation>> _symbolToAnnotationsMap = new();

        public static SyntaxAnnotation GetOrCreateAnnotation( string kind, ISymbol symbol )
        {
            var list = _symbolToAnnotationsMap.GetOrCreateValue( symbol );

            lock ( list )
            {
                var annotation = list.SingleOrDefault( x => x.Kind == kind );

                if ( annotation == null )
                {
                    annotation = new SyntaxAnnotation( kind );
                    list.Add( annotation );
                    _annotationToSymbolMap.Add( annotation, new WeakReference<ISymbol>( symbol ) );
                }

                return annotation;
            }
        }

        private static ISymbol GetSymbolFromAnnotation( SyntaxAnnotation annotation )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !_annotationToSymbolMap.TryGetValue( annotation, out var reference ) || !reference.TryGetTarget( out var symbol ) )
            {
                throw new KeyNotFoundException();
            }

            return symbol;
        }

        public static ExpressionSyntax AddExpressionTypeAnnotation( ExpressionSyntax node, ITypeSymbol? type )
        {
            if ( type == null )
            {
                return node;
            }

            var existingAnnotation = node.GetAnnotations( ExpressionTypeAnnotationKind ).SingleOrDefault();

            if ( existingAnnotation != null && SymbolEqualityComparer.IncludeNullability.Equals( GetSymbolFromAnnotation( existingAnnotation ), type ) )
            {
                return node;
            }
            
            var syntaxAnnotation = GetOrCreateAnnotation( ExpressionTypeAnnotationKind, type );

            Invariant.Assert( SymbolEqualityComparer.IncludeNullability.Equals( GetSymbolFromAnnotation( syntaxAnnotation ), type ) );

            ExpressionSyntax AddAnnotationRecursive( ExpressionSyntax n )
            {
                if ( n is ParenthesizedExpressionSyntax parenthesizedExpression )
                {
                    return parenthesizedExpression.WithExpression( AddAnnotationRecursive( parenthesizedExpression.Expression ) )
                        .WithoutAnnotations( ExpressionTypeAnnotationKind )
                        .WithAdditionalAnnotations( syntaxAnnotation );
                }
                else
                {
                    return n.WithoutAnnotations( ExpressionTypeAnnotationKind ).WithAdditionalAnnotations( syntaxAnnotation );
                }
            }

            return AddAnnotationRecursive( node );
        }

        public static bool TryFindExpressionTypeFromAnnotation(
            SyntaxNode node,
            CompilationContext compilationContext,
            [NotNullWhen( true )] out ITypeSymbol? type )
        {
            // If we don't know the exact type, check if we have a type annotation on the syntax.

            var typeAnnotation = node.GetAnnotations( ExpressionTypeAnnotationKind ).FirstOrDefault();

            if ( typeAnnotation != null! )
            {
                type = (ITypeSymbol) GetSymbolFromAnnotation( typeAnnotation );
            }
            else
            {
                type = null;

                return false;
            }

            type = compilationContext.SymbolTranslator.Translate( type ).AssertNotNull( $"The symbol '{type}' could not be translated." );

            return true;
        }
    }
}