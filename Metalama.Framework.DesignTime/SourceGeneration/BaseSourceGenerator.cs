﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
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

        protected ServiceProvider<IGlobalService> ServiceProvider { get; }

        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<ProjectKey, ProjectHandler?> _projectHandlers = new();
        private readonly TouchIdComparer _touchIdComparer;

        protected BaseSourceGenerator( ServiceProvider<IGlobalService> serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
            this._logger = serviceProvider.GetLoggerFactory().GetLogger( this.GetType().Name );
            this._touchIdComparer = new TouchIdComparer( this._logger );
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

                this._logger.Trace?.Log( $"Initialize()" );

                var source =
                    context.AnalyzerConfigOptionsProvider.Select(
                            ( x, _ ) =>
                            {
                                var msBuildProjectOptions = MSBuildProjectOptionsFactory.Default.GetInstance( x );
                                this._logger.Trace?.Log( $"Roslyn asks the generated source for '{msBuildProjectOptions.AssemblyName}'." );

                                return (AnalyzerOptions: x.GlobalOptions, PipelineOptions: msBuildProjectOptions);
                            } )
                        .Combine( context.CompilationProvider )
                        .Combine( context.AdditionalTextsProvider.Select( ( text, _ ) => text ).Collect() )
                        .Select( ( x, _ ) => (Compilation: x.Left.Right, x.Left.Left.AnalyzerOptions, x.Left.Left.PipelineOptions, AdditionalTexts: x.Right) )
                        .Select( this.OnGeneratedSourceRequested )
                        .WithComparer( this._touchIdComparer )
                        .Select(
                            ( x, cancellationToken )
                                => x.Options == null
                                    ? SourceGeneratorResult.Empty
                                    : this.GetGeneratedSources( x.Compilation, x.Options, cancellationToken.ToTestable() ) );

                context.RegisterSourceOutput( source, ( productionContext, result ) => result.ProduceContent( productionContext ) );

                this._logger.Trace?.Log( $"Initialize(): completed." );
            }
            catch ( Exception e ) when ( DesignTimeExceptionHandler.MustHandle( e ) )
            {
                DesignTimeExceptionHandler.ReportException( e );

                // We rethrow the exception because it is important that the user knows that there was a problem,
                // given that the compilation may be broken.
                throw;
            }
        }

        private (MSBuildProjectOptions? Options, Compilation Compilation, string? TouchId) OnGeneratedSourceRequested(
            (Compilation Compilation, AnalyzerConfigOptions AnalyzerOptions, MSBuildProjectOptions PipelineOptions, ImmutableArray<AdditionalText>
                AdditionalTexts) args,
            CancellationToken cancellationToken )
        {
            this._logger.Trace?.Log( $"OnGeneratedSourceRequested('{args.Compilation.AssemblyName}')" );

            if ( !args.AnalyzerOptions.TryGetValue( $"build_property.AssemblyName", out var assemblyNameFromOptions )
                 || string.IsNullOrEmpty( assemblyNameFromOptions ) )
            {
                return (null, args.Compilation, null);
            }

            this.OnGeneratedSourceRequested( args.Compilation, args.PipelineOptions, cancellationToken.ToTestable() );

            var touchId = GetTouchId( args.AnalyzerOptions, args.AdditionalTexts, cancellationToken );

            this._logger.Trace?.Log( $"OnGeneratedSourceRequested('{args.Compilation.AssemblyName}'): touchId = '{touchId}'" );

            return (args.PipelineOptions, args.Compilation, touchId);
        }

        /// <summary>
        /// This method is called every time the source generator is called. If must decide if the cached result can be served. It must also, if necessary, schedule
        /// a background computation of the compilation.
        /// </summary>
        protected abstract void OnGeneratedSourceRequested(
            Compilation compilation,
            MSBuildProjectOptions options,
            TestableCancellationToken cancellationToken );

        protected SourceGeneratorResult GetGeneratedSources(
            Compilation compilation,
            MSBuildProjectOptions options,
            TestableCancellationToken cancellationToken )
        {
            this._logger.Trace?.Log( $"GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )})." );

            if ( !options.IsFrameworkEnabled )
            {
                // Metalama is not enabled for this project.
                this._logger.Trace?.Log(
                    $"GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): Metalama not enabled." );

                return SourceGeneratorResult.Empty;
            }

            var projectKey = compilation.GetProjectKey();

            if ( !projectKey.HasHashCode )
            {
                this._logger.Warning?.Log(
                    $"GetGeneratedSources('{options.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): no syntax tree." );

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
                $"GetGeneratedSources('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): returned {result}." );

            return result;
        }

        private class TouchIdComparer : IEqualityComparer<(MSBuildProjectOptions? Options, Compilation Compilation, string? TouchId)>
        {
            private readonly ILogger _logger;

            public TouchIdComparer( ILogger logger )
            {
                this._logger = logger;
            }

            public bool Equals(
                (MSBuildProjectOptions? Options, Compilation Compilation, string? TouchId) x,
                (MSBuildProjectOptions? Options, Compilation Compilation, string? TouchId) y )
            {
                var equals = x.TouchId == y.TouchId;

                this._logger.Trace?.Log( $"TouchIdComparer('{x.Options?.AssemblyName}') '{x.TouchId}' {(equals ? "==" : "!=")} '{y.TouchId}'" );

                return equals;
            }

            public int GetHashCode( (MSBuildProjectOptions? Options, Compilation Compilation, string? TouchId) obj ) => obj.TouchId?.GetHashCodeOrdinal() ?? 0;
        }

        private static string GetTouchId(
            AnalyzerConfigOptions options,
            ImmutableArray<AdditionalText> additionalTexts,
            CancellationToken cancellationToken )
        {
            if ( !options.TryGetValue( $"build_property.{MSBuildPropertyNames.MetalamaSourceGeneratorTouchFile}", out var touchFilePath )
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