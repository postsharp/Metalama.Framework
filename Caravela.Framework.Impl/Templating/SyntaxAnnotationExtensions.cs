// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        private const string _buildTimeAnnotationData = "buildTime";
        private const string _runTimeAnnotationData = "runTime";
        private const string _dynamicAnnotationData = "compileTimeDynamic";
        private const string _runTimeDynamicAnnotationData = "runTimeDynamic";
        private const string _unknownAnnotationData = "unknown";
        private const string _bothAnnotationData = "both";

        private static readonly SyntaxAnnotation _buildTimeOnlyAnnotation = new( _scopeAnnotationKind, _buildTimeAnnotationData );
        private static readonly SyntaxAnnotation _runTimeOnlyAnnotation = new( _scopeAnnotationKind, _runTimeAnnotationData );
        private static readonly SyntaxAnnotation _compileTimeDynamicAnnotation = new( _scopeAnnotationKind, _dynamicAnnotationData );
        private static readonly SyntaxAnnotation _runTimeDynamicAnnotation = new( _scopeAnnotationKind, _runTimeDynamicAnnotationData );
        private static readonly SyntaxAnnotation _bothAnnotation = new( _scopeAnnotationKind, _bothAnnotationData );
        private static readonly SyntaxAnnotation _unknownAnnotation = new( _scopeAnnotationKind, _unknownAnnotationData );
        private static readonly SyntaxAnnotation _templateAnnotation = new( _templateAnnotationKind );
        private static readonly SyntaxAnnotation _noDeepIndentAnnotation = new( _noIndentAnnotationKind );
        private static readonly SyntaxAnnotation _scopeMismatchAnnotation = new( _scopeMismatchKind );

        private static readonly ImmutableList<string> _templateAnnotationKinds =
            SyntaxTreeAnnotationMap.AnnotationKinds.AddRange(
                new[] { _scopeAnnotationKind, _noIndentAnnotationKind, _proceedAnnotationKind, _colorAnnotationKind } );

        public static bool HasScopeAnnotation( this SyntaxNode node )
        {
            return node.HasAnnotations( _scopeAnnotationKind );
        }

        public static TemplatingScope? GetScopeFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _scopeAnnotationKind ).SingleOrDefault();

            // No annotation means it is default scope usable for both (runTime or compileTime)
            if ( annotation == null )
            {
                return null;
            }

            switch ( annotation.Data )
            {
                case _buildTimeAnnotationData:
                    return TemplatingScope.CompileTimeOnly;

                case _runTimeAnnotationData:
                    return TemplatingScope.RunTimeOnly;

                case _unknownAnnotationData:
                    return TemplatingScope.Unknown;

                case _dynamicAnnotationData:
                    return TemplatingScope.CompileTimeDynamic;

                case _runTimeDynamicAnnotationData:
                    return TemplatingScope.Dynamic;

                case _bothAnnotationData:
                    return TemplatingScope.Both;

                default:
                    throw new AssertionFailedException();
            }
        }

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxNodeOrToken node )
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

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxNode node ) => ((SyntaxNodeOrToken) node).GetColorFromAnnotation();

        public static TextSpanClassification GetColorFromAnnotation( this SyntaxToken token ) => ((SyntaxNodeOrToken) token).GetColorFromAnnotation();

        public static T AddColoringAnnotation<T>( this T node, TextSpanClassification color )
            where T : SyntaxNode
            => (T) ((SyntaxNodeOrToken) node).AddColoringAnnotation( color ).AsNode()!;

        public static SyntaxToken AddColoringAnnotation( this SyntaxToken token, TextSpanClassification color )
            => ((SyntaxNodeOrToken) token).AddColoringAnnotation( color ).AsToken();

        public static SyntaxNodeOrToken AddColoringAnnotation( this SyntaxNodeOrToken node, TextSpanClassification color )
        {
            if ( color == TextSpanClassification.Default || node.GetColorFromAnnotation() >= color )
            {
                return node;
            }

            var transformedNode = node.WithoutAnnotations( _colorAnnotationKind )
                .WithAdditionalAnnotations( new SyntaxAnnotation( _colorAnnotationKind, color.ToString() ) );

            Invariant.Assert( transformedNode.GetColorFromAnnotation() == color );

            return transformedNode;
        }

        [return: NotNullIfNotNull( "node" )]
        public static T? ReplaceScopeAnnotation<T>( this T? node, TemplatingScope? scope )
            where T : SyntaxNode
        {
            if ( node == null )
            {
                return null;
            }

            return node.WithoutAnnotations( _scopeAnnotationKind ).AddScopeAnnotation( scope );
        }

        [return: NotNullIfNotNull( "node" )]
        public static T? AddScopeAnnotation<T>( this T? node, TemplatingScope? scope )
            where T : SyntaxNode
        {
            if ( node == null )
            {
                return null;
            }

            if ( scope == null )
            {
                return node;
            }

            if ( node.HasAnnotations( _scopeAnnotationKind ) && scope != node.GetScopeFromAnnotation() )
            {
                throw new AssertionFailedException(
                    $"The scope of the {node.Kind()} has already been set to {node.GetScopeFromAnnotation()} and cannot be changed to {scope}." );
            }

            switch ( scope )
            {
                case TemplatingScope.CompileTimeOnly:
                    return node.WithAdditionalAnnotations( _buildTimeOnlyAnnotation );

                case TemplatingScope.RunTimeOnly:
                    return node.WithAdditionalAnnotations( _runTimeOnlyAnnotation );

                case TemplatingScope.Unknown:
                    return node.WithAdditionalAnnotations( _unknownAnnotation );

                case TemplatingScope.CompileTimeDynamic:
                    return node.WithAdditionalAnnotations( _compileTimeDynamicAnnotation );

                case TemplatingScope.Dynamic:
                    return node.WithAdditionalAnnotations( _runTimeDynamicAnnotation );

                case TemplatingScope.Both:
                    return node.WithAdditionalAnnotations( _bothAnnotation );

                default:
                    throw new AssertionFailedException();
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
            => node.WithAdditionalAnnotations( source.GetAnnotations( SyntaxTreeAnnotationMap.AnnotationKinds ) );

        public static T WithTemplateAnnotationsFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( source.GetAnnotations( _templateAnnotationKinds ) );

        public static T AddNoDeepIndentAnnotation<T>( this T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( _noDeepIndentAnnotation );

        public static bool HasNoDeepIndentAnnotation( this SyntaxNode node ) => node.HasAnnotation( _noDeepIndentAnnotation );
    }
}