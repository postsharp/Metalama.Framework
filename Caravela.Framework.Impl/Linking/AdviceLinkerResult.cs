// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal record AdviceLinkerResult(
        CSharpCompilation Compilation,
        ImmutableDiagnosticList Diagnostics );
}
