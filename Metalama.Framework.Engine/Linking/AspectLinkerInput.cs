﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Transformations;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking;

/// <summary>
/// Input of the aspect linker.
/// </summary>
internal readonly struct AspectLinkerInput
{
    /// <summary>
    /// Gets the input compilation model, modified by all aspects.
    /// </summary>
    public CompilationModel CompilationModel { get; }

    public CompilationModel SourceCompilationModel { get; }

    /// <summary>
    /// Gets a list of non-observable transformations.
    /// </summary>
    public IReadOnlyCollection<ITransformation> Transformations { get; }

    /// <summary>
    /// Gets a list of ordered aspect layers.
    /// </summary>
    public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }

    public CompileTimeProject CompileTimeProject { get; }

    public AspectLinkerInput(
        CompilationModel compilationModel,
        CompilationModel sourceCompilationModel,
        IReadOnlyCollection<ITransformation> transformations,
        IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
        CompileTimeProject compileTimeProject )
    {
        this.CompilationModel = compilationModel;
        this.SourceCompilationModel = sourceCompilationModel;
        this.Transformations = transformations;
        this.OrderedAspectLayers = orderedAspectLayers;
        this.CompileTimeProject = compileTimeProject;
    }
}