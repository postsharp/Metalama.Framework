// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Templating
{
    internal static class SyntaxAnnotationExtensions
    {
        private const string _scopeAnnotationKind = "scope";
        private const string _proceedAnnotationKind = "proceed";
        private const string _noIndentAnnotationKind = "noIndent";
        private const string _colorAnnotationKind = "color";
        private const string _templateAnnotationKind = "template";
        private const string _scopeMismatchKind = "scopeMismatch";

        private static readonly SyntaxAnnotation _buildTimeOnlyAnnotation = new( _scopeAnnotationKind, "buildTime" );
        private static readonly SyntaxAnnotation _runTimeOnlyAnnotation = new( _scopeAnnotationKind, "runTime" );
        private static readonly SyntaxAnnotation _unknownAnnotation = new( _scopeAnnotationKind, "unknown" );
        private static readonly SyntaxAnnotation _templateAnnotation = new( _templateAnnotationKind );
        private static readonly SyntaxAnnotation _noDeepIndentAnnotation = new( _noIndentAnnotationKind );
        private static readonly SyntaxAnnotation _scopeMismatchAnnotation = new( _scopeMismatchKind );

        private static readonly ImmutableList<string> _templateAnnotationKinds =
            SemanticAnnotationMap.AnnotationKinds.AddRange(
                new[] { _scopeAnnotationKind, _noIndentAnnotationKind, _proceedAnnotationKind, _colorAnnotationKind } );

        public static bool HasScopeAnnotation( this SyntaxNode node )
        {
            return node.HasAnnotations( _scopeAnnotationKind );
        }

        public static SymbolDeclarationScope GetScopeFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _scopeAnnotationKind ).SingleOrDefault();

            if ( annotation == null )
            {
                return SymbolDeclarationScope.Both;
            }

            switch ( annotation.Data )
            {
                case "buildTime":
                    return SymbolDeclarationScope.CompileTimeOnly;

                case "runTime":
                    return SymbolDeclarationScope.RunTimeOnly;

                case "unknown":
                    return SymbolDeclarationScope.Unknown;

                default:
                    throw new AssertionFailedException();
            }
        }

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _colorAnnotationKind ).SingleOrDefault();

            if ( annotation == null )
            {
                return TextSpanClassification.Default;
            }

            if ( Enum.TryParse( annotation.Data, out TextSpanClassification color ) )
            {
                return color;
            }

            return TextSpanClassification.Default;
        }

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxToken node )
        {
            var annotation = node.GetAnnotations( _colorAnnotationKind ).SingleOrDefault();

            if ( annotation == null )
            {
                return TextSpanClassification.Default;
            }

            if ( Enum.TryParse( annotation.Data, out TextSpanClassification color ) )
            {
                return color;
            }

            return TextSpanClassification.Default;
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

        [return: NotNullIfNotNull( "node" )]
        public static T? AddScopeAnnotation<T>( this T? node, SymbolDeclarationScope scope )
            where T : SyntaxNode
        {
            if ( node == null )
            {
                return null;
            }

            if ( scope == SymbolDeclarationScope.Both )
            {
                return node;
            }

            var existingScope = node.GetScopeFromAnnotation();

            if ( existingScope != SymbolDeclarationScope.Both )
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

                case SymbolDeclarationScope.Unknown:
                    return node.WithAdditionalAnnotations( _unknownAnnotation );

                default:
                    return node;
            }
        }

        public static T AddIsTemplateAnnotation<T>( this T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( _templateAnnotation );

        public static bool IsTemplateFromAnnotation( this SyntaxNode node ) => node.HasAnnotation( _templateAnnotation );

        public static T AddScopeMismatchAnnotation<T>( this T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( _scopeMismatchAnnotation );

        public static bool HasScopeMismatchAnnotation( this SyntaxNode node ) => node.HasAnnotation( _scopeMismatchAnnotation );

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