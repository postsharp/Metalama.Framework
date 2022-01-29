// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

public class AnalysisProcessProjectHandler : ProjectHandler
{
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly ILogger _logger;

    private volatile CancellationTokenSource? _currentCancellationSource;

    private ImmutableDictionary<string, IntroducedSyntaxTree>? _lastIntroducedTrees;

    protected ImmutableDictionary<string, SourceText>? Sources { get; private set; }

    public AnalysisProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions ) : base( serviceProvider, projectOptions )
    {
        this._pipelineFactory = this.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._logger = this.ServiceProvider.GetLoggerFactory().GetLogger( "DesignTime" );
    }

    public override void GenerateSources( Compilation compilation, GeneratorExecutionContext context )
    {
        void AddSources()
        {
            if ( this.Sources != null )
            {
                foreach ( var source in this.Sources )
                {
                    this._logger.Trace?.Log( $"  AddSource('{source.Key}')" );

                    context.AddSource( source.Key, source.Value );
                }
            }
        }

        if ( this.Sources != null )
        {
            this._logger.Trace?.Log( "Serving the generated sources from the cache." );

            // Serve from the cache.
            AddSources();

            // Atomically cancel the previous computation and create a new cancellation token.
            CancellationToken newCancellationToken;

            while ( true )
            {
                var currentCancellationSource = this._currentCancellationSource;
                var newCancellationSource = new CancellationTokenSource();
                
                // It's critical to take the token before calling CompareExchange, otherwise the source may be disposed.
                newCancellationToken = newCancellationSource.Token;

                if ( Interlocked.CompareExchange( ref this._currentCancellationSource, newCancellationSource, currentCancellationSource )
                     == currentCancellationSource )
                {
                    // We won the race. Cancel the previous task if any.
                    
                    currentCancellationSource?.Cancel();
                    currentCancellationSource?.Dispose();

                    break;
                }
                else
                {
                    // We lost the race. Continue iterating.
                }
            }

            // Schedule a new computation.
            
            _ = Task.Run( () => this.ComputeAndPublishAsync( compilation, newCancellationToken ), newCancellationToken );
        }
        else
        {
            // We don't have sources in the cache.

            this._logger.Trace?.Log( $"No generated sources in the cache for project '{this.ProjectOptions.ProjectId}'. Need to generate them synchronously." );

            if ( this.Compute( compilation, context.CancellationToken ) )
            {
                AddSources();

                // Publish the changes asynchronously.
                var cancellationSource = this._currentCancellationSource = new CancellationTokenSource();
                _ = Task.Run( () => this.PublishAsync( cancellationSource.Token ), cancellationSource.Token );
            }
        }
    }

    private bool Compute( Compilation compilation, CancellationToken cancellationToken )
    {
        // Execute the pipeline.
        if ( !this._pipelineFactory.TryExecute(
                this.ProjectOptions,
                compilation,
                cancellationToken,
                out var compilationResult ) )
        {
            this._logger.Warning?.Log(
                $"{this.GetType().Name}.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed." );

            this._logger.Trace?.Log(
                " Compilation references: " + string.Join(
                    ", ",
                    compilation.References.GroupBy( r => r.GetType() ).Select( g => $"{g.Key.Name}: {g.Count()}" ) ) );

            return false;
        }

        // Check if the pipeline returned any difference. If not, do not update our cache.
        if ( compilationResult.PipelineResult.IntroducedSyntaxTrees == this._lastIntroducedTrees )
        {
            this._logger.Trace?.Log(
                $"{this.GetType().Name}.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): generated sources did not change." );

            return false;
        }

        this._lastIntroducedTrees = compilationResult.PipelineResult.IntroducedSyntaxTrees;

        // Cache the introduces trees.
        var generatedSources = ImmutableDictionary.CreateBuilder<string, SourceText>();

        foreach ( var introducedSyntaxTree in this._lastIntroducedTrees! )
        {
            var sourceText = introducedSyntaxTree.Value.GeneratedSyntaxTree.GetText();
            generatedSources[introducedSyntaxTree.Value.Name] = sourceText;
        }

        this.Sources = generatedSources.ToImmutable();

        this._logger.Trace?.Log(
            $"{this.GetType().Name}.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): {this._lastIntroducedTrees!.Count} sources generated." );

        return true;
    }

    private async Task ComputeAndPublishAsync( Compilation compilation, CancellationToken cancellationToken )
    {
        if ( this.Compute( compilation, cancellationToken ) )
        {
            await this.PublishAsync( cancellationToken );
        }
    }

    private async Task PublishAsync( CancellationToken cancellationToken )
    {
        this._logger.Trace?.Log( $"{this.GetType().Name}.Publish('{this.ProjectOptions.ProjectId}'" );

        // Publish to the interactive process. We need to await before we change the touch file.
        await this.PublishGeneratedSourcesAsync( this.ProjectOptions.ProjectId, cancellationToken );

        // Notify Roslyn that we have changes.
        if ( this.ProjectOptions.SourceGeneratorTouchFile == null )
        {
            this._logger.Error?.Log( "Property MetalamaSourceGeneratorTouchFile cannot be null." );
        }
        else
        {
            this.UpdateTouchFile();
        }
    }

    protected void UpdateTouchFile()
    {
        this._logger.Trace?.Log( $"Touching '{this.ProjectOptions.SourceGeneratorTouchFile}'." );
        RetryHelper.Retry( () => File.WriteAllText( this.ProjectOptions.SourceGeneratorTouchFile, Guid.NewGuid().ToString() ) );
    }

    protected virtual Task PublishGeneratedSourcesAsync( string projectId, CancellationToken cancellationToken ) => Task.CompletedTask;

    protected override void Dispose( bool disposing )
    {
        base.Dispose( disposing );
        this._currentCancellationSource?.Dispose();
    }
}