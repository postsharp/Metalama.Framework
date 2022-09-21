// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using AnalyzerConfigOptions = Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions;

namespace Metalama.Framework.DesignTime.SourceGeneration
{
    /// <summary>
    /// Our base implementation of <see cref="ISourceGenerator"/>, which essentially delegates the work to a <see cref="ProjectHandler"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract partial class BaseSourceGenerator : IIncrementalGenerator
    {
        static BaseSourceGenerator()
        {
            DesignTimeServices.Initialize();
        }

        protected ServiceProvider ServiceProvider { get; }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<ProjectKey, ProjectHandler?> _projectHandlers = new();

        protected BaseSourceGenerator( ServiceProvider serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
        }

        protected abstract ProjectHandler CreateSourceGeneratorImpl( IProjectOptions projectOptions, ProjectKey projectKey );

        void IIncrementalGenerator.Initialize( IncrementalGeneratorInitializationContext context )
        {
            try
            {
                if ( MetalamaCompilerInfo.IsActive )
                {
                    return;
                }

                this._logger.Trace?.Log( $"{this.GetType().Name}.Initialize()" );

                var source =
                    context.AnalyzerConfigOptionsProvider.Select(
                            ( x, _ ) => (AnalyzerOptions: x.GlobalOptions, PipelineOptions: MSBuildProjectOptions.GetInstance( x )) )
                        .Combine( context.CompilationProvider )
                        .Combine( context.AdditionalTextsProvider.Select( ( text, _ ) => text ).Collect() )
                        .Select( ( x, _ ) => (Compilation: x.Left.Right, x.Left.Left.AnalyzerOptions, x.Left.Left.PipelineOptions, AdditionalTexts: x.Right) )
                        .Select( this.OnGeneratedSourceRequested )
                        .WithComparer( TouchIdComparer.Instance )
                        .Select( ( x, cancellationToken ) => this.GetGeneratedSources( x.Compilation, x.Options, cancellationToken ) );

                context.RegisterSourceOutput( source, ( productionContext, result ) => result.ProduceContent( productionContext ) );

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

        private (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) OnGeneratedSourceRequested(
            (Compilation Compilation, AnalyzerConfigOptions AnalyzerOptions, MSBuildProjectOptions PipelineOptions, ImmutableArray<AdditionalText>
                AdditionalTexts) args,
            CancellationToken cancellationToken )
        {
            this.OnGeneratedSourceRequested( args.Compilation, args.PipelineOptions, cancellationToken );

            var touchId = GetTouchId( args.AnalyzerOptions, args.AdditionalTexts, cancellationToken );

            return (args.PipelineOptions, args.Compilation, touchId);
        }

        /// <summary>
        /// This method is called every time the source generator is called. If must decide if the cached result can be served. It must also, if necessary, schedule
        /// a background computation of the compilation.
        /// </summary>
        protected abstract void OnGeneratedSourceRequested( Compilation compilation, MSBuildProjectOptions options, CancellationToken cancellationToken );

        protected SourceGeneratorResult GetGeneratedSources(
            Compilation compilation,
            MSBuildProjectOptions options,
            CancellationToken cancellationToken )
        {
            this._logger.Trace?.Log(
                $"{this.GetType().Name}.GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

            if ( !options.IsFrameworkEnabled )
            {
                // Metalama is not enabled for this project.
                this._logger.Trace?.Log(
                    $"{this.GetType().Name}.GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): Metalama not enabled." );

                return SourceGeneratorResult.Empty;
            }

            var projectKey = compilation.GetProjectKey();

            if ( !projectKey.HasHashCode )
            {
                this._logger.Warning?.Log(
                    $"{this.GetType().Name}.GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): no syntax tree." );

                return SourceGeneratorResult.Empty;
            }

            // Get or create an IProjectHandler instance.
            if ( !this._projectHandlers.TryGetValue( projectKey, out var projectHandler ) )
            {
                projectHandler = this._projectHandlers.GetOrAdd(
                    projectKey,
                    _ =>
                    {
                        if ( options.IsFrameworkEnabled )
                        {
                            if ( options.IsDesignTimeEnabled )
                            {
                                return this.CreateSourceGeneratorImpl( options, projectKey );
                            }
                            else
                            {
                                return new OfflineProjectHandler( this.ServiceProvider, options, projectKey );
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

        private class TouchIdComparer : IEqualityComparer<(MSBuildProjectOptions Options, Compilation Compilation, string TouchId)>
        {
            public static readonly TouchIdComparer Instance = new();

            public bool Equals(
                (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) x,
                (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) y )
                => x.TouchId == y.TouchId;

            public int GetHashCode( (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) obj ) => obj.TouchId.GetHashCodeOrdinal();
        }

        private static string GetTouchId(
            AnalyzerConfigOptions options,
            ImmutableArray<AdditionalText> additionalTexts,
            CancellationToken cancellationToken )
        {
            if ( !options.TryGetValue( $"build_property.MetalamaSourceGeneratorTouchFile", out var touchFilePath )
                 || string.IsNullOrWhiteSpace( touchFilePath ) )
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