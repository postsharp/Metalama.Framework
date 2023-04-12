// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Compiler;
using Metalama.Framework.Engine.AspectWeavers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Formatting
{
    /// <summary>
    /// Exposes the <see cref="SyntaxAnnotation"/>s used by Metalama.
    /// </summary>
    [PublicAPI]
    public static class FormattingAnnotations
    {
        /// <summary>
        /// Annotation used to mark locals and 'return;' statement that may be redundant. Currently we are not doing anything with them,
        /// but we could.
        /// </summary>
        public static SyntaxAnnotation PossibleRedundantAnnotation { get; } = new( "Metalama_PossibleRedundant" );

        /// <summary>
        /// Gets an annotation that means that the code has been generated by Metalama, but not by an aspect.
        /// </summary>
        internal static SyntaxAnnotation SystemGeneratedCodeAnnotation { get; } = new( MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind, null );

        /// <summary>
        /// Gets the kind of annotations that means that the code has been generated by Metalama or by an aspect.
        /// </summary>
        internal static string GeneratedCodeAnnotationKind => MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind;

        private static SyntaxAnnotation? _simplifier;

        /// <summary>
        /// Gets an annotation that means that the syntax stems from source code. This can be added to a child node of a node annotated
        /// with the <c>WithGeneratedCodeAnnotation</c> method. This is a synonym for <see cref="MetalamaCompilerAnnotations.SourceCodeAnnotation"/>.
        /// </summary>
        public static SyntaxAnnotation SourceCodeAnnotation => MetalamaCompilerAnnotations.SourceCodeAnnotation;

        /// <summary>
        /// Gets an annotation that means that a qualified type must be simplified. This is equivalent to <c>Simplifier.Annotation</c>.
        /// </summary>
        internal static void Initialize( SyntaxAnnotation value )
        {
            // This property must be set by the engine assembly because we don't want a dependency on workspaces here.
            _simplifier = value;
        }

        public static T WithSimplifierAnnotation<T>( this T node ) where T : SyntaxNode => node.WithAdditionalAnnotations( _simplifier );

        [return: NotNullIfNotNull( "node" )]
        private static T? WithAnnotationInsideBlock<T>( this T? node, SyntaxAnnotation annotation, bool addToBrackets = false )
            where T : SyntaxNode
        {
            switch ( node )
            {
                case BlockSyntax block:
                    var annotatedBlock = (T) (object) block.WithStatements(
                        SyntaxFactory.List( block.Statements.Select( s => s.WithAnnotationInsideBlock( annotation, true ) ) ) );

                    if ( addToBrackets )
                    {
                        annotatedBlock = annotatedBlock.WithAdditionalAnnotations( annotation );
                    }

                    return annotatedBlock;

                default:
                    return node?.WithAdditionalAnnotations( annotation );
            }
        }

        /// <summary>
        /// Annotates a syntax node with an annotation meaning that the syntax node and all its children are generated.
        /// The annotation is typically obtained from <see cref="AspectWeaverContext.GeneratedCodeAnnotation"/>, but it can also
        /// be created from <see cref="MetalamaCompilerAnnotations.CreateGeneratedCodeAnnotation"/>.
        /// </summary>
        /// <param name="node">The input node.</param>
        /// <param name="annotation">A <see cref="SyntaxAnnotation"/> of kind <see cref="MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind"/>.</param>
        /// <returns>The annotated node.</returns>
        public static SyntaxToken WithGeneratedCodeAnnotation( this SyntaxToken node, SyntaxAnnotation annotation )
            => node.WithAdditionalAnnotations( annotation );

        /// <summary>
        /// Annotates a syntax node with an annotation meaning that the syntax node and all its children are generated, except whose marked with <see cref="WithSourceCodeAnnotation{T}"/>.
        /// The annotation is typically obtained from <see cref="AspectWeaverContext.GeneratedCodeAnnotation"/>, but it can also
        /// be created from <see cref="MetalamaCompilerAnnotations.CreateGeneratedCodeAnnotation"/>.
        /// </summary>
        /// <param name="node">The input node.</param>
        /// <param name="annotation">A <see cref="SyntaxAnnotation"/> of kind <see cref="MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind"/>.</param>
        /// <returns>The annotated node.</returns>
        [return: NotNullIfNotNull( "node" )]
        public static T? WithGeneratedCodeAnnotation<T>( this T? node, SyntaxAnnotation annotation )
            where T : SyntaxNode
            => node?.WithAnnotationInsideBlock( annotation );

        /// <summary>
        /// Annotates a syntax node with an annotation meaning that the syntax node and all its children are user code.
        /// The annotation is typically obtained from <see cref="AspectWeaverContext.GeneratedCodeAnnotation"/>, but it can also
        /// be created from <see cref="MetalamaCompilerAnnotations.CreateGeneratedCodeAnnotation"/>.
        /// </summary>
        /// <param name="node">The input node.</param>
        /// <returns>The annotated node.</returns>
        [return: NotNullIfNotNull( "node" )]
        public static T? WithSourceCodeAnnotation<T>( this T? node )
            where T : SyntaxNode
            => node?.WithAnnotationInsideBlock( SourceCodeAnnotation );

        [return: NotNullIfNotNull( "node" )]
        public static T WithSourceCodeAnnotationIfNotGenerated<T>( this T node )
            where T : SyntaxNode
            => !node.HasAnnotations( MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind )
                ? node.WithSourceCodeAnnotation()
                : node;

        public static T WithFormattingAnnotationsFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
        {
            if ( source.HasAnnotation( SourceCodeAnnotation ) )
            {
                return node.WithSourceCodeAnnotation();
            }
            else if ( source.GetAnnotations( MetalamaCompilerAnnotations.GeneratedCodeAnnotationKind ).FirstOrDefault() is { } annotation )
            {
                return node.WithGeneratedCodeAnnotation( annotation );
            }
            else
            {
                return node;
            }
        }
    }
}