// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

public interface IAspectPipelineConfigurationProvider : IService
{
    bool TryGetConfiguration(
        PartialCompilation compilation,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out AspectPipelineConfiguration? configuration );
}