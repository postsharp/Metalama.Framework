// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.Pipeline
{
    /// <summary>
    /// Results produced by <see cref="Metalama.Framework.Engine.DesignTime.Pipeline.DesignTimeAspectPipeline"/>.
    /// </summary>
    /// <param name="Success">Determines whether the pipeline was successful.</param>
    /// <param name="InputSyntaxTrees">The syntax trees for which the pipeline was executed.</param>
    /// <param name="IntroducedSyntaxTrees">The syntax trees introduced by the pipeline (for source generators).</param>
    /// <param name="Diagnostics">The list of diagnostics and suppressions.</param>
    internal record DesignTimeAspectPipelineResult(
        bool Success,
        ImmutableDictionary<string, SyntaxTree> InputSyntaxTrees,
        IReadOnlyList<IntroducedSyntaxTree> IntroducedSyntaxTrees,
        ImmutableUserDiagnosticList Diagnostics,
        IReadOnlyList<AttributeAspectInstance> InheritableAspects );
}