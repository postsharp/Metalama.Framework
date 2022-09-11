// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

public class StandaloneCodeFixRunner : CodeFixRunner
{
    private readonly CompileTimeDomain _domain;
    private readonly ServiceProvider _serviceProvider;

    public StandaloneCodeFixRunner( CompileTimeDomain domain, ServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._domain = domain;
        this._serviceProvider = serviceProvider;
    }

    private protected override 
        ValueTask<(bool Success, AspectPipelineConfiguration? Configuration, ServiceProvider? ServiceProvider, CompileTimeDomain? Domain)>
        GetConfigurationAsync(
            PartialCompilation compilation,
            CancellationToken cancellationToken )
    {
        return new( (true, null, this._serviceProvider, this._domain) );
    }
}