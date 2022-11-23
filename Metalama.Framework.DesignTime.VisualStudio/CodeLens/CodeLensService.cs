// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeLens;
using Metalama.Framework.DesignTime.Contracts.CodeLens;
using Metalama.Framework.DesignTime.VisualStudio.Remoting.UserProcess;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio.CodeLens;

internal class CodeLensService : ICodeLensService
{
    private readonly UserProcessServiceHubEndpoint _userProcessEndpoint;

    public CodeLensService( GlobalServiceProvider serviceProvider )
    {
        this._userProcessEndpoint = serviceProvider.GetRequiredService<UserProcessServiceHubEndpoint>();
    }

    public async Task GetCodeLensSummaryAsync(
        Compilation compilation,
        ISymbol symbol,
        ICodeLensSummary?[] result,
        CancellationToken cancellationToken = default )
    {
        var projectKey = compilation.GetProjectKey();

        if ( !projectKey.IsMetalamaEnabled )
        {
            result[0] = CodeLensSummary.NoAspect;

            return;
        }

        result[0] =
            await (await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.GetCodeLensSummaryAsync), cancellationToken ))
                .GetCodeLensSummaryAsync(
                    projectKey,
                    symbol.GetSerializableId(),
                    cancellationToken );
    }

    public async Task GetCodeLensDetailsAsync(
        Compilation compilation,
        ISymbol symbol,
        ICodeLensDetails?[] result,
        CancellationToken cancellationToken = default )
    {
        var projectKey = compilation.GetProjectKey();

        if ( !projectKey.IsMetalamaEnabled )
        {
            result[0] = CodeLensDetailsTable.Empty;

            return;
        }

        result[0] =
            await (await this._userProcessEndpoint.GetApiAsync( projectKey, nameof(this.GetCodeLensSummaryAsync), cancellationToken ))
                .GetCodeLensDetailsAsync(
                    projectKey,
                    symbol.GetSerializableId(),
                    cancellationToken );
    }
}