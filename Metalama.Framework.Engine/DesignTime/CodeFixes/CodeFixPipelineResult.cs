// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes;

internal sealed record CodeFixPipelineResult(
    AspectPipelineConfiguration Configuration,
    CompilationModel Compilation,
    ImmutableArray<CodeFixInstance> CodeFixes );