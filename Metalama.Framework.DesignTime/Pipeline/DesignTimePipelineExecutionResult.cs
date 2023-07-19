// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Results produced by <see cref="DesignTimeAspectPipeline"/>.
    /// </summary>
    /// <param name="InputSyntaxTrees">The syntax trees for which the pipeline was executed.</param>
    /// <param name="IntroducedSyntaxTrees">The syntax trees introduced by the pipeline (for source generators).</param>
    /// <param name="Diagnostics">The list of diagnostics and suppressions.</param>
    internal sealed record DesignTimePipelineExecutionResult(
        ImmutableDictionary<string, SyntaxTree> InputSyntaxTrees,
        IReadOnlyList<IntroducedSyntaxTree> IntroducedSyntaxTrees,
        ImmutableUserDiagnosticList Diagnostics,
        IReadOnlyList<InheritableAspectInstance> InheritableAspects,
        ImmutableArray<ReferenceValidatorInstance> ReferenceValidators,
        ImmutableArray<IAspectInstance> AspectInstances,
        IReadOnlyCollection<ITransformationBase> Transformations );
}