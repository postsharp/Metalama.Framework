// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Offline;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime;

public partial class DesignTimeSourceGenerator
{
    private class OfflineSourceGeneratorImpl : SourceGeneratorImpl
    {
        public override void GenerateSources( IProjectOptions projectOptions, Compilation compilation, GeneratorExecutionContext context )
        {
            var serviceProvider = ServiceProvider.Empty.WithServices( projectOptions );
            var provider = new AdditionalCompilationOutputFileProvider( serviceProvider );

            if ( projectOptions.AdditionalCompilationOutputDirectory == null )
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

            Logger.DesignTime.Trace?.Log( $"DesignTimeSourceGenerator.Execute('{context.Compilation.AssemblyName}'): {sourcesCount} sources generated." );
        }
    }
}