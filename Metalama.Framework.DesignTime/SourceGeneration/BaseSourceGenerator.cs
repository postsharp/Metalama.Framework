﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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

                IEqualityComparer<(AnalyzerConfigOptionsProvider Options, Compilation Compilation, string TouchId)> comparer;

                var source =
                    context.AnalyzerConfigOptionsProvider.Select( (x, _ ) => ( AnalyzerOptions: x.GlobalOptions, PipelineOptions: new MSBuildProjectOptions( x.GlobalOptions )) )
                        .Combine( context.CompilationProvider )
                        .Combine( context.AdditionalTextsProvider.Select( ( text, _ ) => text ).Collect() )
                        .Select( (x, _) => (Compilation: x.Left.Right, x.Left.Left.AnalyzerOptions,  x.Left.Left.PipelineOptions, AdditionalTexts: x.Right )  )
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

        private (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) OnGeneratedSourceRequested( (Compilation Compilation, AnalyzerConfigOptions AnalyzerOptions, MSBuildProjectOptions PipelineOptions, ImmutableArray<AdditionalText> AdditionalTexts) args, CancellationToken cancellationToken )
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

            if ( string.IsNullOrEmpty( options.ProjectId ) )
            {
                // Metalama is not enabled for this project.

                return SourceGeneratorResult.Empty;
            }

            // Get or create an IProjectHandler instance.
            if ( !this._projectHandlers.TryGetValue( options.ProjectId, out var projectHandler ) )
            {
                projectHandler = this._projectHandlers.GetOrAdd(
                    options.ProjectId,
                    _ =>
                    {
                        if ( options.IsFrameworkEnabled )
                        {
                            if ( options.IsDesignTimeEnabled )
                            {
                                return this.CreateSourceGeneratorImpl( options );
                            }
                            else
                            {
                                return new OfflineProjectHandler( this.ServiceProvider, options );
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

            public int GetHashCode( (MSBuildProjectOptions Options, Compilation Compilation, string TouchId) obj )
                => obj.TouchId.GetHashCode();
        }

        private static string GetTouchId(
            AnalyzerConfigOptions options,
            ImmutableArray<AdditionalText> additionalTexts,
            CancellationToken cancellationToken )
        {
            if ( !options.TryGetValue( $"build_property.MetalamaSourceGeneratorTouchFile", out var touchFilePath ) )
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