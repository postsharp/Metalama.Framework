// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Offline;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime;

public partial class TheSourceGenerator
{
    private class OfflineProjectHandler : ProjectHandler
    {
        private readonly ILogger _logger;

        public OfflineProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions ) : base( serviceProvider, projectOptions )
        {
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        public override void GenerateSources( Compilation compilation, GeneratorExecutionContext context )
        {
            var serviceProvider = Engine.Pipeline.ServiceProvider.Empty.WithServices( this.ProjectOptions );

            var provider = new AdditionalCompilationOutputFileProvider( serviceProvider );

            if ( this.ProjectOptions.AdditionalCompilationOutputDirectory == null )
            {
                return;
            }

            var sourcesCount = 0;

            foreach ( var file in provider.GetAdditionalCompilationOutputFiles()
                         .Where(
                             f => f.Kind == AdditionalCompilationOutputFileKind.DesignTimeGeneratedCode
                                  && StringComparer.Ordinal.Equals( Path.GetExtension( f.Path ), ".cs" ) ) )
            {
                using var stream = file.GetStream();
                context.AddSource( Path.GetFileName( file.Path ), SourceText.From( stream ) );
                sourcesCount++;
            }

            this._logger.Trace?.Log( $"DesignTimeSourceGenerator.Execute('{context.Compilation.AssemblyName}'): {sourcesCount} sources generated." );
        }
    }
}