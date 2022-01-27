using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.CodeFixes;

internal class DesignTimeCodeFixRunner : CodeFixRunner
{
    private readonly IAspectPipelineConfigurationProvider _configurationProvider;

    public DesignTimeCodeFixRunner( IAspectPipelineConfigurationProvider configurationProvider, IProjectOptions projectOptions ) : base( projectOptions )
    {
        this._configurationProvider = configurationProvider;
    }

    private protected override bool TryGetConfiguration(
        PartialCompilation compilation,
        CancellationToken cancellationToken,
        out AspectPipelineConfiguration? configuration,
        [NotNullWhen( true )] out ServiceProvider? serviceProvider,
        [NotNullWhen( true )]out CompileTimeDomain? domain )
    {
        if ( this._configurationProvider.TryGetConfiguration( compilation, NullDiagnosticAdder.Instance, cancellationToken, out configuration ) )
        {
            serviceProvider = configuration.ServiceProvider;
            domain = configuration.Domain;

            return true;
        }
        else
        {
            serviceProvider = null;
            domain = null;

            return false;
        }
    }
}