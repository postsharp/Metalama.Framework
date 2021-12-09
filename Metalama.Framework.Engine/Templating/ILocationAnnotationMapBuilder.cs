// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating
{
    /// <summary>
    /// Annotates syntax nodes with <see cref="Location"/> annotations that can then be resolved by <see cref="ILocationAnnotationMap"/>.
    /// </summary>
    public interface ILocationAnnotationMapBuilder : ILocationAnnotationMap
    {
        SyntaxNode AddLocationAnnotation( SyntaxNode originalNode, SyntaxNode transformedNode );

        SyntaxToken AddLocationAnnotation( SyntaxToken originalToken );

        SyntaxNode AddLocationAnnotationsRecursive( SyntaxNode node );
    }
}