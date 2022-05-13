// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable SA1414 // Tuple items must have names.

namespace Metalama.Framework.DesignTime.SourceGeneration
{
    /// <summary>
    /// Our base implementation of <see cref="ISourceGenerator"/>, which essentially delegates the work to a <see cref="ProjectHandler"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract partial class BaseSourceGenerator : IIncrementalGenerator
    {
        protected ServiceProvider ServiceProvider { get; }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, ProjectHandler?> _projectHandlers = new();

        protected BaseSourceGenerator( ServiceProvider serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        protected abstract ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions );

        void IIncrementalGenerator.Initialize( IncrementalGeneratorInitializationContext context )
        {
            try
            {
                if ( MetalamaCompilerInfo.IsActive )
                {
                    return;
                }

                this._logger.Trace?.Log( $"{this.GetType().Name}.Initialize()" );

                var touchIdProvider =
                    context.AnalyzerConfigOptionsProvider.Select( ( options, _ ) => options )
                        .Combine( context.AdditionalTextsProvider.Select( ( text, _ ) => text ).Collect() )
                        .Select( ( x, cancellationToken ) => (TouchId: GetTouchId( x.Left, x.Right, cancellationToken ), Options: x.Left) );

                var generatedSourcesProvider =
                    context.CompilationProvider.Combine( touchIdProvider )
                        .WithComparer( TouchIdComparer.Instance )
                        .Select( ( x, cancellationToken ) => this.GetGeneratedSources( x.Item1, x.Item2.Options, cancellationToken ) );

                context.RegisterSourceOutput( generatedSourcesProvider, ( productionContext, result ) => result.ProduceContent( productionContext ) );

                this._logger.Trace?.Log( $"{this.GetType().Name}.Initialize(): completed." );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );

                // We rethrow the exception because it is important that the user knows that there was a problem,
                // given that the compilation may be broken.
                throw;
            }
        }

        private SourceGeneratorResult GetGeneratedSources(
            Compilation compilation,
            AnalyzerConfigOptionsProvider options,
            CancellationToken cancellationToken )
        {
            this._logger.Trace?.Log(
                $"{this.GetType().Name}.GetGeneratedSources('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

            var projectOptions = new ProjectOptions( options );

            if ( string.IsNullOrEmpty( projectOptions.ProjectId ) )
            {
                // Metalama is not enabled for this project.

                return SourceGeneratorResult.Empty;
            }

            // Get or create an IProjectHandler instance.
            if ( !this._projectHandlers.TryGetValue( projectOptions.ProjectId, out var projectHandler ) )
            {
                projectHandler = this._projectHandlers.GetOrAdd(
                    projectOptions.ProjectId,
                    _ =>
                    {
                        if ( projectOptions.IsFrameworkEnabled )
                        {
                            if ( projectOptions.IsDesignTimeEnabled )
                            {
                                return this.CreateSourceGeneratorImpl( projectOptions );
                            }
                            else
                            {
                                return new OfflineProjectHandler( this.ServiceProvider, projectOptions );
                            }
                        }
                        else
                        {
                            return null;
                        }
                    } );
            }

            if ( projectHandler == null )
            {
                return SourceGeneratorResult.Empty;
            }

            // Invoke GenerateSources for the project handler.
            var result = projectHandler.GenerateSources( compilation, cancellationToken );

            this._logger.Trace?.Log(
                $"{this.GetType().Name}.GetGeneratedSources('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): returned {result}." );

            return result;
        }

        private class TouchIdComparer : IEqualityComparer<(Compilation Compilation, (string TouchId, AnalyzerConfigOptionsProvider Options))>
        {
            public static readonly TouchIdComparer Instance = new();

            public bool Equals(
                (Compilation Compilation, (string TouchId, AnalyzerConfigOptionsProvider Options)) x,
                (Compilation Compilation, (string TouchId, AnalyzerConfigOptionsProvider Options)) y )
                => x.Item2.TouchId == y.Item2.TouchId;

            public int GetHashCode( (Compilation Compilation, (string TouchId, AnalyzerConfigOptionsProvider Options)) obj ) => obj.Item2.TouchId.GetHashCode();
        }

        private static string GetTouchId(
            AnalyzerConfigOptionsProvider options,
            ImmutableArray<AdditionalText> additionalTexts,
            CancellationToken cancellationToken )
        {
            if ( !options.GlobalOptions.TryGetValue( $"build_property.MetalamaSourceGeneratorTouchFile", out var touchFilePath ) )
            {
                return "";
            }

            var normalizedTouchFilePath = Path.GetFullPath( touchFilePath );
            var touchText = additionalTexts.FirstOrDefault( x => x.Path.Equals( normalizedTouchFilePath, StringComparison.OrdinalIgnoreCase ) );

            if ( touchText == null )
            {
                return "";
            }

            return touchText.GetText( cancellationToken )?.ToString() ?? "";
        }
    }
}