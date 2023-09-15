// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.AspectConfiguration;

internal interface IConfiguratorSource
{
    IEnumerable<Configurator> GetConfigurators( CompilationModel compilation, IDiagnosticAdder diagnosticAdder );
}