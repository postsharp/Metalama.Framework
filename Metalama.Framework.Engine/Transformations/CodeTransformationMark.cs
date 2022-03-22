// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a mark on a syntax node that will be a target of code transformations.
    /// </summary>
    internal struct CodeTransformationMark
    {
        /// <summary>
        /// Gets the transformation that provided this mark.
        /// </summary>
        public ICodeTransformation Source { get; }

        /// <summary>
        /// Gets the target syntax node for this mark. 
        /// </summary>
        public SyntaxNode Target { get; }

        /// <summary>
        /// Gets the operator for this code transformation.
        /// </summary>
        public CodeTransformationOperator Operator { get; }

        /// <summary>
        /// Gets the operand syntax.
        /// </summary>
        public SyntaxNode? Operand { get; }

        public CodeTransformationMark( ICodeTransformation source, SyntaxNode target, CodeTransformationOperator @operator, SyntaxNode? operand)
        {
            this.Source = source;
            this.Target = target;
            this.Operator = @operator;
            this.Operand = operand;
        }
    }
}