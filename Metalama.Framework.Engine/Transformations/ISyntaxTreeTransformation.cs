// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any introduction to the code model that modifies a syntax tree. 
    /// </summary>
    internal interface ISyntaxTreeTransformation : ITransformation
    {
        /// <summary>
        /// Gets the syntax trees that needs to be modified.
        /// </summary>
        ImmutableArray<SyntaxTree> TargetSyntaxTrees { get; }
    }
}