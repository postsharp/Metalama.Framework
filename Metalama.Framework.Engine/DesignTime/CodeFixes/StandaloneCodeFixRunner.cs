// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes;

public sealed class StandaloneCodeFixRunner : CodeFixRunner
{
    private readonly CompileTimeDomain _domain;
    private readonly ProjectServiceProvider _serviceProvider;

    public StandaloneCodeFixRunner( CompileTimeDomain domain, ProjectServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._domain = domain;
        this._serviceProvider = serviceProvider;
    }

    private protected override
        ValueTask<(bool Success, AspectPipelineConfiguration? Configuration, ProjectServiceProvider? ServiceProvider, CompileTimeDomain? Domain)>
        GetConfigurationAsync(
            PartialCompilation compilation,
            TestableCancellationToken cancellationToken )
        => new( (true, null, this._serviceProvider, this._domain) );
}