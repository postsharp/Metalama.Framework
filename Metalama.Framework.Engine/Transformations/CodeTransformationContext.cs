// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Context for code transformation's syntax node evaluation.
    /// </summary>
    internal struct CodeTransformationContext
    {
        private readonly List<CodeTransformationMark> _marks;

        public bool IsDeclined { get; private set; }

        internal ICodeTransformation Transformation { get; }

        public SyntaxNode Target { get; }

        internal IReadOnlyList<CodeTransformationMark> Marks => this._marks;

        public CodeTransformationContext( ICodeTransformation transformation, SyntaxNode target)
        {
            this._marks = new List<CodeTransformationMark>();
            this.Transformation = transformation;
            this.Target = target;
            this.IsDeclined = false;
        }

        /// <summary>
        /// Adds a transformation mark for the current node.
        /// </summary>
        /// <param name="operator"></param>
        /// <param name="operand"></param>
        public void AddMark( CodeTransformationOperator @operator, SyntaxNode? operand )
        {
            this._marks.Add( new CodeTransformationMark(this.Transformation, this.Target, @operator, operand ) );
        }

        /// <summary>
        /// When called indicates that the transformation does not need to visit further child syntax nodes.
        /// </summary>
        public void Decline()
        {
            this.IsDeclined = true;
        }
    }
}