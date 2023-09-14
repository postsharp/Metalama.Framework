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

    public ProgrammaticConfiguratorSource(
        Type optionsType,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<UserOptionsConfigurator>> getInstances )
    {
        this.OptionsTypes = ImmutableArray.Create( optionsType );
        this._getInstances = getInstances;
    }

    public ImmutableArray<Type> OptionsTypes { get; }

    public IEnumerable<UserOptionsConfigurator> GetConfigurators( Type optionsType, CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        if ( optionsType != this.OptionsTypes[0] )
        {
            return Enumerable.Empty<UserOptionsConfigurator>();
        }
        else
        {
            return this._getInstances( compilation, diagnosticAdder );
        }
    }
}