// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.UserOptions;

internal interface IConfiguratorSource
{
    ImmutableArray<string> OptionTypes { get; }

    IEnumerable<UserOptionsConfigurator> GetConfigurators( string optionsTypeName, CompilationModel compilation, IDiagnosticAdder diagnosticAdder );
}