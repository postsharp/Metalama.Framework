// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

internal interface IAspectPipelineConfigurationProvider
{
    bool TryGetConfiguration(
        PartialCompilation compilation,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken,
        [NotNullWhen( true )] out AspectPipelineConfiguration? configuration );
}