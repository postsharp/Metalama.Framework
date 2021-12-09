// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Templating
{
    internal partial class MetaSyntaxRewriter
    {
        /// <summary>
        /// Specifies how a <see cref="SyntaxNode"/> must be transformed.
        /// </summary>
        protected enum TransformationKind
        {
            /// <summary>
            /// No transformation. The original node is returned.
            /// </summary>
            None,

            /// <summary>
            /// The original node is cloned. This kind of transformation is currently only used
            /// to validate that the generated code is correct.
            /// </summary>
            Clone,

            /// <summary>
            /// The original node is transformed, i.e. the <c>Visit</c> method returns
            /// an expression that evaluates to an instance equivalent to the source one.
            /// </summary>
            Transform
        }
    }
}