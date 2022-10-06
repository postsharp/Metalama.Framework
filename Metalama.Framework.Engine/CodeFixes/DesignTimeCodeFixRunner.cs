// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Project;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

public class DesignTimeCodeFixRunner : CodeFixRunner
{
    private readonly IAspectPipelineConfigurationProvider _configurationProvider;

    public DesignTimeCodeFixRunner( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._configurationProvider = serviceProvider.GetRequiredService<IAspectPipelineConfigurationProvider>();
    }

    private protected override
        async ValueTask<(bool Success, AspectPipelineConfiguration? Configuration, ServiceProvider? ServiceProvider, CompileTimeDomain? Domain)>
        GetConfigurationAsync(
            PartialCompilation compilation,
            CancellationToken cancellationToken )
    {
        var getConfigurationResult = await this._configurationProvider.GetConfigurationAsync( compilation, cancellationToken );

        if ( getConfigurationResult.IsSuccess )
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