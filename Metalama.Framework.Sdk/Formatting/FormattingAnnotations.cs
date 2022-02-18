// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Formatting
{
    public static class FormattingAnnotations
    {
        private static SyntaxAnnotation? _simplifier;

        /// <summary>
        /// Gets an annotation that means that the syntax has been generated by Metalama. This is used to selectively format the code,
        /// and can be used in the future for syntax highlighting.
        /// </summary>
        public static SyntaxAnnotation GeneratedCode { get; } = new( "Metalama_Generated" );

        /// <summary>
        /// Gets an annotation that means that the syntax stems from source code. This can be added to a child node of a node annotated
        /// with <see cref="GeneratedCode"/>.
        /// </summary>
        public static SyntaxAnnotation SourceCode { get; } = new( "Metalama_SourceCode" );

        /// <summary>
        /// Gets an annotation that means that a qualified type must be simplified. This is equivalent to <c>Simplifier.Annotation</c>.
        /// </summary>
        public static SyntaxAnnotation Simplify
        {
            get => _simplifier ?? throw new InvalidOperationException();

            // This property must be set by the engine assembly because we don't want a dependency on workspaces here.
            internal set => _simplifier = value;
        }

        [return: NotNullIfNotNull( "node" )]
        private static T? AddAnnotationInsideBlock<T>( this T? node, SyntaxAnnotation annotation, bool addToBrackets = false )
            where T : SyntaxNode
        {
            switch ( node )
            {
                case BlockSyntax block:
                    var annotatedBlock = (T) (object) block.WithStatements(
                        SyntaxFactory.List( block.Statements.Select( s => s.AddAnnotationInsideBlock( annotation, true ) ) ) );

                    if ( addToBrackets )
                    {
                        annotatedBlock = annotatedBlock.WithAdditionalAnnotations( annotation );
                    }

                    return annotatedBlock;

                default:
                    return node?.WithAdditionalAnnotations( annotation );
            }
        }

        public static SyntaxToken AddGeneratedCodeAnnotation( this SyntaxToken node ) => node.WithAdditionalAnnotations( GeneratedCode );

        [return: NotNullIfNotNull( "node" )]
        public static T? AddGeneratedCodeAnnotation<T>( this T? node )
            where T : SyntaxNode
            => node?.AddAnnotationInsideBlock( GeneratedCode );

        [return: NotNullIfNotNull( "node" )]
        public static SyntaxTrivia AddGeneratedCodeAnnotation( this SyntaxTrivia node )
            => node.WithAdditionalAnnotations( GeneratedCode );

        [return: NotNullIfNotNull( "node" )]
        public static T? AddSourceCodeAnnotation<T>( this T? node )
            where T : SyntaxNode
            => node?.AddAnnotationInsideBlock( SourceCode );

        [return: NotNullIfNotNull( "node" )]
        public static SyntaxTrivia AddSourceCodeAnnotation( this SyntaxTrivia node )
            => node.WithAdditionalAnnotations( SourceCode );

        public static T WithFormattingAnnotationsFrom<T>( this T node, SyntaxNode source )
            where T : SyntaxNode
        {
            if ( source.HasAnnotation( SourceCode ) )
            {
                return node.AddSourceCodeAnnotation();
            }
            else if ( source.HasAnnotation( GeneratedCode ) )
            {
                return node.AddGeneratedCodeAnnotation();
            }
            else
            {
                return node;
            }
        }
    }
}