// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any introduction to the code model that modifies a syntax tree. 
    /// </summary>
    internal interface ISyntaxTreeTransformation : ITransformation
    {
        /// <summary>
        /// Gets the syntax tree that needs to be modified.
        /// </summary>
        SyntaxTree TargetSyntaxTree { get; }
    }
}