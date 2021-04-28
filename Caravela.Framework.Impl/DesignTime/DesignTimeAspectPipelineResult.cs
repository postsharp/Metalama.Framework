// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Results produced by <see cref="Caravela.Framework.Impl.DesignTime.DesignTimeAspectPipeline"/>.
    /// </summary>
    /// <param name="Success">Determines whether the pipeline was successful.</param>
    /// <param name="InputSyntaxTrees">The syntax trees for which the pipeline was executed.</param>
    /// <param name="IntroducedSyntaxTrees">The syntax trees introduced by the pipeline (for source generators).</param>
    /// <param name="Diagnostics">The list of diagnostics and suppressions.</param>
    internal record DesignTimeAspectPipelineResult(
        bool Success,
        IReadOnlyCollection<SyntaxTree> InputSyntaxTrees,
        IReadOnlyList<IntroducedSyntaxTree> IntroducedSyntaxTrees,
        ImmutableDiagnosticList Diagnostics );
}