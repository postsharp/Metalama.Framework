// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal sealed class ProgrammaticHierarchicalOptionsSource : IHierarchicalOptionsSource
{
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<HierarchicalOptionsInstance>> _getInstances;

    public ProgrammaticHierarchicalOptionsSource( Func<CompilationModel, IDiagnosticAdder, IEnumerable<HierarchicalOptionsInstance>> getInstances )
    {
        this._getInstances = getInstances;
    }

    public IEnumerable<HierarchicalOptionsInstance> GetOptions( CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        return this._getInstances( compilation, diagnosticAdder );
    }
}