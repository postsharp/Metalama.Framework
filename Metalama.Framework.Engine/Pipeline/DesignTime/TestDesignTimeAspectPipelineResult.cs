// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

// ReSharper disable NotAccessedPositionalProperty.Global
internal sealed record TestDesignTimeAspectPipelineResult(
    bool Success,
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableArray<IntroducedSyntaxTree> AdditionalSyntaxTrees );