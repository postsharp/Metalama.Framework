// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents any transformation.
/// </summary>
internal interface ITransformation
{
    SyntaxTree TransformedSyntaxTree { get; }

    IDeclaration TargetDeclaration { get; }

    Advice ParentAdvice { get; }

    int OrderWithinPipelineStepAndTypAndAspectInstance { get; set; }

    int OrderWithinPipelineStepAndType { get; set; }

    int OrderWithinPipeline { get; set; }

    TransformationObservability Observability { get; }

    TransformationKind TransformationKind { get; }

    FormattableString ToDisplayString();
}

internal enum TransformationObservability
{
    None,
    CompileTimeOnly,
    Always
}