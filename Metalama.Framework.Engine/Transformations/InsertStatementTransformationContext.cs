// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Context for code transformation's syntax node evaluation.
    /// </summary>
    internal readonly struct InsertStatementTransformationContext
    {
        public IInsertStatementTransformation Transformation { get; }

        public ISymbol DeclarationSymbol { get; }

        /// <summary>
        /// Gets the target syntax node. If the target declaration does not have a body this is going to be null.
        /// </summary>
        public SyntaxNode? Body { get; }

        public InsertStatementTransformationContext( IInsertStatementTransformation transformation, ISymbol declarationSymbol, SyntaxNode? body )
        {
            this.Transformation = transformation;
            this.DeclarationSymbol = declarationSymbol;
            this.Body = body;
        }
    }
}