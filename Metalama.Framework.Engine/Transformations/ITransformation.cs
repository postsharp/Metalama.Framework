// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Transformations
{
    /// <summary>
    /// Represents any transformation.
    /// </summary>
    internal interface ITransformation
    {
        SyntaxTree TransformedSyntaxTree { get; }

        IDeclaration TargetDeclaration { get; }

        Advice ParentAdvice { get; }

        int OrderWithinAspectInstance { get; set; }
    }
}