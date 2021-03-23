// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    internal record DesignTimeAspectPipelineResult(
        IImmutableDictionary<string, SyntaxTree>? AdditionalSyntaxTrees,
        ImmutableDiagnosticList? Diagnostics
    );
}