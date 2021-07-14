// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Provides extension methods for handling of linker annotations.
    /// </summary>
    internal static class LinkerAnnotationExtensions
    {
        [return: NotNullIfNotNull( "node" )]
        public static T? AddSourceCodeAnnotation<T>( this T? node )
            where T : SyntaxNode
            => node switch
            {
                BlockSyntax block => (T) (object) block.AddSourceCodeAnnotation(),
                _ => node?.WithAdditionalAnnotations( AspectPipelineAnnotations.SourceCode )
            };

        [return: NotNullIfNotNull( "node" )]
        public static BlockSyntax? AddSourceCodeAnnotation( this BlockSyntax? node )
            => node?.WithStatements( SyntaxFactory.List( node.Statements.Select( s => s.WithAdditionalAnnotations( AspectPipelineAnnotations.SourceCode ) ) ) )
                .WithAdditionalAnnotations( AspectPipelineAnnotations.SourceCode );

        [return: NotNullIfNotNull( "node" )]
        public static ArrowExpressionClauseSyntax? AddSourceCodeAnnotation( this ArrowExpressionClauseSyntax? node )
            => node?.WithExpression( AddSourceCodeAnnotation( node.Expression ) );

        [return: NotNullIfNotNull( "node" )]
        public static AccessorListSyntax? AddSourceCodeAnnotation( this AccessorListSyntax? node )
            => node?.WithAccessors(
                SyntaxFactory.List(
                    node.Accessors.Select(
                        a => a.WithBody( a.Body.AddSourceCodeAnnotation() )
                            .WithExpressionBody( a.ExpressionBody.AddSourceCodeAnnotation() ) ) ) );
    }
}