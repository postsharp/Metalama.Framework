// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal sealed class ProgrammaticHierarchicalOptionsSource : IHierarchicalOptionsSource
{
    private readonly Func<CompilationModel, AspectResultCollector, CancellationToken, Task> _collectOptionsAction;

    public ProgrammaticHierarchicalOptionsSource( Func<CompilationModel, AspectResultCollector, CancellationToken, Task> collectOptionsAction )
    {
        this._collectOptionsAction = collectOptionsAction;
    }

    public Task CollectOptionsAsync( CompilationModel compilation, AspectResultCollector collector, CancellationToken cancellationToken )
    {
        return this._collectOptionsAction( compilation, collector, cancellationToken );
    }
}