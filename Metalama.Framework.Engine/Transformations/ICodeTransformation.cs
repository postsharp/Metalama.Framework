// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents a single code transformation.
    /// </summary>
    internal interface ICodeTransformation : INonObservableTransformation
    {
        ITransformation Parent { get; }

        /// <summary>
        /// Gets a target method base of this code transformation.
        /// </summary>
        IMethodBase TargetDeclaration { get; }

        /// <summary>
        /// Evaluates the target syntax node and transforms the state.
        /// </summary>
        /// <param name="context"></param>
        void EvaluateSyntaxNode( CodeTransformationContext context );
    }
}