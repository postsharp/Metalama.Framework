// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Project;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

public interface IAspectPipelineConfigurationProvider : IService
{
    ValueTask<AspectPipelineConfiguration?> GetConfigurationAsync(
        PartialCompilation compilation,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken );
}