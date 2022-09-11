// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline.CompileTime
{
    public record CompileTimeAspectPipelineResult(
        ImmutableArray<SyntaxTreeTransformation> SyntaxTreeTransformations,
        ImmutableArray<ManagedResource> AdditionalResources,
        IPartialCompilation ResultingCompilation,
        ImmutableArray<AdditionalCompilationOutputFile> AdditionalCompilationOutputFiles );
}