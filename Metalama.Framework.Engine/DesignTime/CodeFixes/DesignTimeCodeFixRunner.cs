// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes;

public sealed class DesignTimeCodeFixRunner : CodeFixRunner
{
    private readonly IAspectPipelineConfigurationProvider _configurationProvider;

    public DesignTimeCodeFixRunner( in ProjectServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._configurationProvider = serviceProvider.Global.GetRequiredService<IAspectPipelineConfigurationProvider>();
    }

    private protected override
        async ValueTask<(bool Success, AspectPipelineConfiguration? Configuration, ProjectServiceProvider? ServiceProvider, CompileTimeDomain? Domain)>
        GetConfigurationAsync(
            PartialCompilation compilation,
            TestableCancellationToken cancellationToken )
    {
        var getConfigurationResult = await this._configurationProvider.GetConfigurationAsync( compilation, AsyncExecutionContext.Get(), cancellationToken );

        if ( getConfigurationResult.IsSuccessful )
        {
            var configuration = getConfigurationResult.Value;

            return (true, configuration, configuration.ServiceProvider, configuration.Domain);
        }
        else
        {
            return default;
        }
    }
}