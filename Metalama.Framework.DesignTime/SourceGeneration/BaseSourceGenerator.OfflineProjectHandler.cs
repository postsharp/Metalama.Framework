// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Offline;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.SourceGeneration;

public partial class BaseSourceGenerator
{
    private class OfflineProjectHandler : ProjectHandler
    {
        private readonly ILogger _logger;

        public OfflineProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey ) : base(
            serviceProvider,
            projectOptions,
            projectKey )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        public override SourceGeneratorResult GenerateSources( Compilation compilation, CancellationToken cancellationToken )
        {
            var serviceProvider = Engine.Pipeline.ServiceProvider.Empty.WithServices( this.ProjectOptions );

            var provider = new AdditionalCompilationOutputFileProvider( serviceProvider );

            if ( this.ProjectOptions.AdditionalCompilationOutputDirectory == null )
            {
                return SourceGeneratorResult.Empty;
            }

            var result = new OfflineSourceGeneratorResult(
                provider.GetAdditionalCompilationOutputFiles()
                    .Where(
                        f => f.Kind == AdditionalCompilationOutputFileKind.DesignTimeGeneratedCode
                             && StringComparer.Ordinal.Equals( Path.GetExtension( f.Path ), ".cs" ) )
                    .ToImmutableArray() );

            this._logger.Trace?.Log( $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}'): {result.OfflineFiles.Length} sources generated." );

            return result;
        }
    }
}