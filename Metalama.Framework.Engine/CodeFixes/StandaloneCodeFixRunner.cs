// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.CodeFixes;

public class StandaloneCodeFixRunner : CodeFixRunner
{
    private readonly CompileTimeDomain _domain;
    private readonly ServiceProvider _serviceProvider;

    public StandaloneCodeFixRunner( CompileTimeDomain domain, ServiceProvider serviceProvider, IProjectOptions projectOptions ) : base()
    {
        this._domain = domain;
        this._serviceProvider = serviceProvider;
    }

    private protected override bool TryGetConfiguration(
        PartialCompilation compilation,
        CancellationToken cancellationToken,
        out AspectPipelineConfiguration? configuration,
        [NotNullWhen( true )] out ServiceProvider? serviceProvider,
        [NotNullWhen( true )] out CompileTimeDomain? domain )
    {
        configuration = null;
        serviceProvider = this._serviceProvider;
        domain = this._domain;

        return true;
    }
}