// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.UserOptions;

internal sealed class ProgrammaticConfiguratorSource : IConfiguratorSource
{
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<UserOptionsConfigurator>> _getInstances;

    public ProgrammaticConfiguratorSource( Func<CompilationModel, IDiagnosticAdder, IEnumerable<UserOptionsConfigurator>> getInstances )
    {
        this._getInstances = getInstances;
    }

    public IEnumerable<UserOptionsConfigurator> GetConfigurators( CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        return this._getInstances( compilation, diagnosticAdder );
    }
}