// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating
{
    internal static class SyntaxAnnotationExtensions
    {
        private const string _scopeAnnotationKind = "scope";
        private const string _proceedAnnotationKind = "proceed";
        private const string _noindentAnnotationKind = "noindent";
        private const string _colorAnnotationKind = "color";

        private static readonly SyntaxAnnotation _buildTimeOnlyAnnotation = new SyntaxAnnotation( _scopeAnnotationKind, "buildtime" );
        private static readonly SyntaxAnnotation _runTimeOnlyAnnotation = new SyntaxAnnotation( _scopeAnnotationKind, "runtime" );
        private static readonly SyntaxAnnotation _templateAnnotation = new SyntaxAnnotation( _scopeAnnotationKind, "template" );
        private static readonly SyntaxAnnotation _noDeepIndentAnnotation = new SyntaxAnnotation( _noindentAnnotationKind );
        
        private static readonly ImmutableList<string> _templateAnnotationKinds = SemanticAnnotationMap.AnnotationKinds.AddRange( new[] { _scopeAnnotationKind, _noindentAnnotationKind, _proceedAnnotationKind, _colorAnnotationKind } );

        public static bool HasScopeAnnotation( this SyntaxNode node )
        {
            return node.HasAnnotations( _scopeAnnotationKind );
        }

        public static SymbolDeclarationScope GetScopeFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _scopeAnnotationKind ).SingleOrDefault();
            if ( annotation == null )
            {
                return SymbolDeclarationScope.Default;
            }
            else
            {
                switch ( annotation.Data )
                {
                    case "buildtime":
                        return SymbolDeclarationScope.CompileTimeOnly;

                    case "runtime":
                        return SymbolDeclarationScope.RunTimeOnly;

                    case "template":
                        return SymbolDeclarationScope.Template;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _colorAnnotationKind ).SingleOrDefault();
            if ( annotation == null )
            {
                return TextSpanClassification.Default;
            }
            else
            {
                if ( Enum.TryParse( annotation.Data, out TextSpanClassification color ) )
                {
                    return color;
                }
                else
                {
                    return TextSpanClassification.Default;
                }
            }
        }

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxToken node )
        {
            var annotation = node.GetAnnotations( _colorAnnotationKind ).SingleOrDefault();
            if ( annotation == null )
            {
                return TextSpanClassification.Default;
            }
            else
            {
                if ( Enum.TryParse( annotation.Data, out TextSpanClassification color ) )
                {
                    return color;
                }
                else
                {
                    return TextSpanClassification.Default;
                }
            }
        }

        public static T AddColoringAnnotation<T>( this T node, TextSpanClassification color )
            where T : SyntaxNode
        {
            if ( color == TextSpanClassification.Default || node.GetColorFromAnnotation() >= color )
            {
                return node;
            }

            return node.WithoutAnnotations( _colorAnnotationKind )
                .WithAdditionalAnnotations( new SyntaxAnnotation( _colorAnnotationKind, color.ToString() ) );
        }

        public static SyntaxToken AddColoringAnnotation( this SyntaxToken node, TextSpanClassification color )
        {
            if ( color == TextSpanClassification.Default || node.GetColorFromAnnotation() >= color )
            {
                return node;
            }

            return node.WithoutAnnotations( _colorAnnotationKind )
                .WithAdditionalAnnotations( new SyntaxAnnotation( _colorAnnotationKind, color.ToString() ) );
        }

        public static T AddScopeAnnotation<T>( this T node, SymbolDeclarationScope scope )
            where T : SyntaxNode
        {
            if ( scope == SymbolDeclarationScope.Default )
            {
                return node;
            }

            var existingScope = node.GetScopeFromAnnotation();

            if ( existingScope != SymbolDeclarationScope.Default )
            {
                Invariant.Assert( existingScope == scope );
                return node;
            }

            switch ( scope )
            {
                case SymbolDeclarationScope.CompileTimeOnly:
                    return node.WithAdditionalAnnotations( _buildTimeOnlyAnnotation );

                case SymbolDeclarationScope.RunTimeOnly:
                    return node.WithAdditionalAnnotations( _runTimeOnlyAnnotation );

                case SymbolDeclarationScope.Template:
                    return node.WithAdditionalAnnotations( _templateAnnotation );

                default:
                    return node;
            }
        }

        public static T WithScopeAnnotationFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
            => node.AddScopeAnnotation( source.GetScopeFromAnnotation() );

        public static T WithSymbolAnnotationsFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( source.GetAnnotations( SemanticAnnotationMap.AnnotationKinds ) );

        public static T WithTemplateAnnotationsFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( source.GetAnnotations( _templateAnnotationKinds ) );

        public static T AddNoDeepIndentAnnotation<T>( this T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( _noDeepIndentAnnotation );

        public static bool HasNoDeepIndentAnnotation( this SyntaxNode node ) => node.HasAnnotation( _noDeepIndentAnnotation );
    }
}