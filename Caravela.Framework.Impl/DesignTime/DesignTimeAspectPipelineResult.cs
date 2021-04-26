// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.DesignTime
{
    internal record DesignTimeAspectPipelineResult(
        bool Success,
        IReadOnlyCollection<SyntaxTree> InputSyntaxTrees,
        IReadOnlyList<IntroducedSyntaxTree> IntroducedSyntaxTrees,
        ImmutableDiagnosticList Diagnostics ) { }
}