// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Templating
{
    internal static class SyntaxAnnotationExtensions
    {
        public const string ScopeAnnotationKind = "Metalama_Scope";
        private const string _targetScopeAnnotationKind = "Metalama_TargetScope";
        private const string _proceedAnnotationKind = "Metalama_Proceed";
        private const string _noIndentAnnotationKind = "Metalama_NoIndent";
        private const string _colorAnnotationKind = "Metalama_Color";
        private const string _templateAnnotationKind = "Metalama_Template";
        private const string _scopeMismatchKind = nameof(TemplatingScope.Conflict);
        private const string _buildTimeAnnotationData = nameof(TemplatingScope.CompileTimeOnly);
        private const string _runTimeAnnotationData = nameof(TemplatingScope.RunTimeOnly);
        private const string _mustFollowParentAnnotationData = nameof(TemplatingScope.MustFollowParent);
        private const string _compileTimeReturningRunTimeOnlyAnnotationData = nameof(TemplatingScope.CompileTimeOnlyReturningRuntimeOnly);
        private const string _compileTimeReturningBothAnnotationData = nameof(TemplatingScope.CompileTimeOnlyReturningBoth);
        private const string _runTimeDynamicAnnotationData = nameof(TemplatingScope.Dynamic);
        private const string _unknownAnnotationData = nameof(TemplatingScope.LateBound);
        private const string _bothAnnotationData = nameof(TemplatingScope.RunTimeOrCompileTime);
        private const string _runTimeTemplateParameterAnnotationData = nameof(TemplatingScope.RunTimeTemplateParameter);
        private const string _typeOfRunTimeTypeAnnotationData = nameof(TemplatingScope.TypeOfRunTimeType);
        private const string _typeOfGenericTemplateTypeParameterAnnotationData = nameof(TemplatingScope.TypeOfTemplateTypeParameter);

        private static readonly SyntaxAnnotation _buildTimeOnlyAnnotation = new( ScopeAnnotationKind, _buildTimeAnnotationData );
        private static readonly SyntaxAnnotation _runTimeOnlyAnnotation = new( ScopeAnnotationKind, _runTimeAnnotationData );
        private static readonly SyntaxAnnotation _buildTimeTargetAnnotation = new( _targetScopeAnnotationKind, _buildTimeAnnotationData );
        private static readonly SyntaxAnnotation _runTimeTargetAnnotation = new( _targetScopeAnnotationKind, _runTimeAnnotationData );
        private static readonly SyntaxAnnotation _mustFollowParentTargetAnnotation = new( _targetScopeAnnotationKind, _mustFollowParentAnnotationData );

        private static readonly SyntaxAnnotation _compileTimeReturningRunTimeOnlyAnnotation =
            new( ScopeAnnotationKind, _compileTimeReturningRunTimeOnlyAnnotationData );

        private static readonly SyntaxAnnotation _compileTimeReturningBothAnnotation = new( ScopeAnnotationKind, _compileTimeReturningBothAnnotationData );
        private static readonly SyntaxAnnotation _runTimeDynamicAnnotation = new( ScopeAnnotationKind, _runTimeDynamicAnnotationData );
        private static readonly SyntaxAnnotation _bothAnnotation = new( ScopeAnnotationKind, _bothAnnotationData );
        private static readonly SyntaxAnnotation _unknownAnnotation = new( ScopeAnnotationKind, _unknownAnnotationData );
        private static readonly SyntaxAnnotation _templateAnnotation = new( _templateAnnotationKind );
        private static readonly SyntaxAnnotation _noDeepIndentAnnotation = new( _noIndentAnnotationKind );
        private static readonly SyntaxAnnotation _scopeMismatchAnnotation = new( _scopeMismatchKind );
        private static readonly SyntaxAnnotation _runTimeTemplateParameterAnnotation = new( ScopeAnnotationKind, _runTimeTemplateParameterAnnotationData );
        private static readonly SyntaxAnnotation _typeOfRunTimeTypeAnnotation = new( ScopeAnnotationKind, _typeOfRunTimeTypeAnnotationData );

        private static readonly SyntaxAnnotation _typeOfTemplateTypeParameterAnnotation = new(
            ScopeAnnotationKind,
            _typeOfGenericTemplateTypeParameterAnnotationData );

        private static readonly ImmutableList<string> _templateAnnotationKinds =
            SyntaxTreeAnnotationMap.AnnotationKinds.AddRange(
                new[] { ScopeAnnotationKind, _noIndentAnnotationKind, _proceedAnnotationKind, _colorAnnotationKind } );

        public static bool HasScopeAnnotation( this SyntaxNode node ) => node.HasAnnotations( ScopeAnnotationKind );

        public static TemplatingScope? GetScopeFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( ScopeAnnotationKind ).SingleOrDefault();

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
                    return TemplatingScope.LateBound;

                case _compileTimeReturningRunTimeOnlyAnnotationData:
                    return TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;

                case _compileTimeReturningBothAnnotationData:
                    return TemplatingScope.CompileTimeOnlyReturningBoth;

                case _runTimeDynamicAnnotationData:
                    return TemplatingScope.Dynamic;

                case _bothAnnotationData:
                    return TemplatingScope.RunTimeOrCompileTime;

                case _runTimeTemplateParameterAnnotationData:
                    return TemplatingScope.RunTimeTemplateParameter;

                case _typeOfRunTimeTypeAnnotationData:
                    return TemplatingScope.TypeOfRunTimeType;

                case _typeOfGenericTemplateTypeParameterAnnotationData:
                    return TemplatingScope.TypeOfTemplateTypeParameter;

                default:
                    throw new AssertionFailedException( $"Unexpected annotation data: '{annotation.Data}'." );
            }
        }

        public static TemplatingScope GetTargetScopeFromAnnotation( this SyntaxNode node )
        {
            var annotation = node.GetAnnotations( _targetScopeAnnotationKind ).SingleOrDefault();

            // No annotation means it is default scope usable for both (runTime or compileTime)
            if ( annotation == null )
            {
                return TemplatingScope.RunTimeOrCompileTime;
            }

            switch ( annotation.Data )
            {
                case _buildTimeAnnotationData:
                    return TemplatingScope.CompileTimeOnly;

                case _runTimeAnnotationData:
                    return TemplatingScope.RunTimeOnly;

                case _mustFollowParentAnnotationData:
                    return TemplatingScope.MustFollowParent;

                default:
                    throw new AssertionFailedException( $"Unexpected annotation data: '{annotation.Data}'." );
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
            else
            {
                throw new AssertionFailedException( $"Invalid enum value: {annotation.Data}." );
            }
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

            return transformedNode;
        }

        public static T ReplaceScopeAnnotation<T>( this T node, TemplatingScope scope )
            where T : SyntaxNode
        {
            var existingScope = node.GetScopeFromAnnotation().GetValueOrDefault();

            if ( existingScope == scope )
            {
                return node;
            }

            TemplatingScope actualScope;

            if ( existingScope.IsUndetermined() )
            {
                actualScope = scope;
            }
            else
            {
                switch (existingScope, scope)
                {
                    case (TemplatingScope.CompileTimeOnlyReturningBoth, TemplatingScope.RunTimeOnly):
                        actualScope = TemplatingScope.CompileTimeOnlyReturningRuntimeOnly;

                        break;

                    case (TemplatingScope.CompileTimeOnlyReturningBoth, TemplatingScope.CompileTimeOnly):
                        actualScope = TemplatingScope.CompileTimeOnly;

                        break;

                    case (TemplatingScope.CompileTimeOnlyReturningRuntimeOnly, TemplatingScope.CompileTimeOnly):
                        return node;

                    case (TemplatingScope.CompileTimeOnlyReturningRuntimeOnly, TemplatingScope.RunTimeOnly):
                        return node;

                    default:
                        throw new InvalidOperationException(
                            $"Cannot change the scope of node '{node}' to {scope} because it is already set to {existingScope}." );
                }
            }

            return node.WithoutAnnotations( ScopeAnnotationKind ).AddScopeAnnotation( actualScope );
        }

        public static StatementSyntax AddRunTimeOnlyAnnotationIfUndetermined( this StatementSyntax statement )
        {
            if ( statement.GetScopeFromAnnotation().GetValueOrDefault().IsUndetermined() )
            {
                return statement.ReplaceScopeAnnotation( TemplatingScope.RunTimeOnly );
            }
            else
            {
                return statement;
            }
        }

        public static T AddScopeAnnotation<T>( this T node, TemplatingScope? scope )
            where T : SyntaxNode
        {
            if ( scope == null )
            {
                // Coverage: ignore (the API is not consistent without this case even if it is never called).
                return node;
            }

            if ( node.HasAnnotations( ScopeAnnotationKind ) && scope != node.GetScopeFromAnnotation() )
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

                case TemplatingScope.LateBound:
                    return node.WithAdditionalAnnotations( _unknownAnnotation );

                case TemplatingScope.CompileTimeOnlyReturningRuntimeOnly:
                    return node.WithAdditionalAnnotations( _compileTimeReturningRunTimeOnlyAnnotation );

                case TemplatingScope.CompileTimeOnlyReturningBoth:
                    return node.WithAdditionalAnnotations( _compileTimeReturningBothAnnotation );

                case TemplatingScope.Dynamic:
                    return node.WithAdditionalAnnotations( _runTimeDynamicAnnotation );

                case TemplatingScope.RunTimeOrCompileTime:
                    return node.WithAdditionalAnnotations( _bothAnnotation );

                case TemplatingScope.RunTimeTemplateParameter:
                    return node.WithAdditionalAnnotations( _runTimeTemplateParameterAnnotation );

                case TemplatingScope.TypeOfRunTimeType:
                    return node.WithAdditionalAnnotations( _typeOfRunTimeTypeAnnotation );

                case TemplatingScope.TypeOfTemplateTypeParameter:
                    return node.WithAdditionalAnnotations( _typeOfTemplateTypeParameterAnnotation );

                case TemplatingScope.Invalid:
                    // We don't propagate.
                    return node;

                default:
                    throw new AssertionFailedException($"{scope} is not supported.");
            }
        }

        [return: NotNullIfNotNull( "node" )]
        public static T AddTargetScopeAnnotation<T>( this T node, TemplatingScope scope )
            where T : SyntaxNode
        {
            switch ( scope )
            {
                case TemplatingScope.CompileTimeOnly:
                    return node.WithAdditionalAnnotations( _buildTimeTargetAnnotation );

                case TemplatingScope.RunTimeOnly:
                case TemplatingScope.LateBound: // Fall back to RunTimeOnly.
                    return node.WithAdditionalAnnotations( _runTimeTargetAnnotation );

                case TemplatingScope.MustFollowParent:
                    return node.WithAdditionalAnnotations( _mustFollowParentTargetAnnotation );

                default:
                    throw new AssertionFailedException( $"Unexpected value for TemplatingScope: {scope}." );
            }
        }

        public static T AddIsTemplateAnnotation<T>( this T node )
            where T : SyntaxNode
            => node.WithAdditionalAnnotations( _templateAnnotation );

        public static bool IsTemplateFromAnnotation( this SyntaxNode node )
            => node switch
            {
                AccessorDeclarationSyntax accessor => node.HasAnnotation( _templateAnnotation ) || IsTemplateFromAnnotation( accessor.Parent! ),
                _ => node.HasAnnotation( _templateAnnotation )
            };

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