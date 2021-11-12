// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    public record CompileTimeAspectPipelineResult(
        ImmutableArray<SyntaxTreeTransformation> SyntaxTreeTransformations,
        ImmutableArray<ManagedResource> AdditionalResources,
        IPartialCompilation ResultingCompilation,
        ImmutableArray<AuxiliaryFile> AuxiliaryFiles );
}