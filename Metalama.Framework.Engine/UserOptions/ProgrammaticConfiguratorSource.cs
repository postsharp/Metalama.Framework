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
        string optionsType,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<UserOptionsConfigurator>> getInstances )
    {
        this.OptionTypes = ImmutableArray.Create( optionsType );
        this._getInstances = getInstances;
    }

    public ImmutableArray<string> OptionTypes { get; }

    public IEnumerable<UserOptionsConfigurator> GetConfigurators( string optionsType, CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        if ( optionsType != this.OptionTypes[0] )
        {
            return Enumerable.Empty<UserOptionsConfigurator>();
        }
        else
        {
            return this._getInstances( compilation, diagnosticAdder );
        }
    }
}