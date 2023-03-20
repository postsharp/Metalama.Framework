// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;

namespace Metalama.Tool.Divorce;

internal class DivorceCommand : BaseCommand<DivorceCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, DivorceCommandSettings settings )
    {
        var divorceService = new DivorceService( context.ServiceProvider, settings.ProjectPath, settings.Configuration, settings.TargetFramework );

        if ( !settings.Force )
        {
            divorceService.CheckGitStatus();
        }

        divorceService.PerformDivorce();
    }
}