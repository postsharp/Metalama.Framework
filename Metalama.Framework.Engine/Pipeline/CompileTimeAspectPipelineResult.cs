// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    public record CompileTimeAspectPipelineResult(
        ImmutableArray<SyntaxTreeTransformation> SyntaxTreeTransformations,
        ImmutableArray<ManagedResource> AdditionalResources,
        IPartialCompilation ResultingCompilation,
        ImmutableArray<AdditionalCompilationOutputFile> AdditionalCompilationOutputFiles );
}