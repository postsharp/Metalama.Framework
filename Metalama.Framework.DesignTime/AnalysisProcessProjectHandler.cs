// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.SourceGeneration;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

/// <summary>
/// The real implementation of <see cref="ProjectHandler"/>, running in the analysis process. 
/// </summary>
/// <remarks>
/// The implementation works by providing a cached result whenever one is available, and to schedule
/// an asynchronous task to refresh the task. When the task is completed, a touch file is modified,
/// which causes the compiler to call the <see cref="ProjectHandler"/> again, and to receive
/// a fresh copy of the cache.
/// </remarks>
public class AnalysisProcessProjectHandler : ProjectHandler
{
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;
    private readonly IProjectHandlerObserver? _observer;
    private readonly string? _sourceGeneratorTouchFile;
    private readonly ITestableCancellationTokenSourceFactory _testableCancellationTokenSourceFactory;
    private volatile bool _disposed;
    private volatile TestableCancellationTokenSource? _currentCancellationSource;

    public SyntaxTreeSourceGeneratorResult? LastSourceGeneratorResult { get; private set; }

    public AnalysisProcessProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions, ProjectKey projectKey ) : base(
        serviceProvider,
        projectOptions,
        projectKey )
    {
        this._pipelineFactory = this.ServiceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
        this._pipelineFactory.PipelineStatusChangedEvent.RegisterHandler( this.OnPipelineStatusChanged );
        this._observer = this.ServiceProvider.GetService<IProjectHandlerObserver>();
        this._testableCancellationTokenSourceFactory = this.ServiceProvider.GetRequiredService<ITestableCancellationTokenSourceFactory>();

        this._sourceGeneratorTouchFile = this.ProjectOptions.SourceGeneratorTouchFile;

        RetryHelper.Retry( () => Directory.CreateDirectory( Path.GetDirectoryName( this._sourceGeneratorTouchFile )! ) );
    }

    private void OnPipelineStatusChanged( DesignTimePipelineStatusChangedEventArgs args )
    {
        if ( args.Pipeline.ProjectKey == this.ProjectKey )
        {
            this.UpdateTouchFile();
        }
    }

    public override SourceGeneratorResult GenerateSources( Compilation compilation, TestableCancellationToken cancellationToken )
    {
        if ( this.LastSourceGeneratorResult != null )
        {
            this.Logger.Trace?.Log( "Serving the generated sources from the cache." );

            // Atomically cancel the previous computation and create a new cancellation token.
            TestableCancellationToken newCancellationToken;

            while ( true )
            {
                if ( this._disposed )
                {
                    this.Logger.Trace?.Log( "The object has been disposed." );

                    return SourceGeneratorResult.Empty;
                }

                var oldCancellationSource = this._currentCancellationSource;
                var newCancellationSource = this._testableCancellationTokenSourceFactory.Create();

                // It's critical to take the token before calling CompareExchange, otherwise the source may be disposed.
                newCancellationToken = newCancellationSource.Token;

                if ( Interlocked.CompareExchange( ref this._currentCancellationSource, newCancellationSource, oldCancellationSource )
                     == oldCancellationSource )
                {
                    // We won the race. Cancel the previous task if any.

                    oldCancellationSource?.CancellationTokenSource.Cancel();
                    oldCancellationSource?.Dispose();

                    break;
                }
                else
                {
                    // We lost the race. Continue iterating.
                    newCancellationSource.Dispose();

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            // Schedule a new computation.
            this.PendingTasks.Run( () => this.ComputeAndPublishAsync( compilation, newCancellationToken ), newCancellationToken );
        }
        else
        {
            // We don't have sources in the cache.

            this.Logger.Trace?.Log( $"No generated sources in the cache for project '{this.ProjectKey}'. Need to generate them synchronously." );

            if ( TaskHelper.RunAndWait( () => this.ComputeAsync( compilation, cancellationToken ), cancellationToken ) )
            {
                // Publish the changes asynchronously.
                // But do not make publishing cancellable because the user process expects the source to be published from the moment
                // that the analysis process received the source.
                this.PendingTasks.Run( this.PublishAsync, CancellationToken.None );

                this._currentCancellationSource = null;
            }
        }

        // LastSourceGeneratorResult can still be null here if the pipeline failed.
        return this.LastSourceGeneratorResult ?? SourceGeneratorResult.Empty;
    }

    /// <summary>
    /// Executes the pipeline.
    /// </summary>
    private async Task<bool> ComputeAsync( Compilation compilation, TestableCancellationToken cancellationToken )
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pipeline = this._pipelineFactory.GetOrCreatePipeline( this.ProjectOptions, compilation, cancellationToken );

            if ( pipeline == null )
            {
                this.Logger.Warning?.Log(
                    $"{this.GetType().Name}.Execute('{this.ProjectKey}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): cannot get the pipeline." );

                return false;
            }

            var compilationResult = await pipeline.ExecuteAsync( compilation, cancellationToken );

            if ( !compilationResult.IsSuccessful )
            {
                this.Logger.Warning?.Log(
                    $"{this.GetType().Name}.Execute('{this.ProjectKey}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed." );

                this.Logger.Trace?.Log(
                    " Compilation references: " + string.Join(
                        ", ",
                        compilation.References.GroupBy( r => r.GetType() ).Select( g => $"{g.Key.Name}: {g.Count()}" ) ) );

                return false;
            }

            var newSourceGeneratorResult = new SyntaxTreeSourceGeneratorResult( compilationResult.Value.TransformationResult.IntroducedSyntaxTrees );

            // Check if the pipeline returned any difference. If not, do not update our cache.
            if ( this.LastSourceGeneratorResult != null && this.LastSourceGeneratorResult.Equals( newSourceGeneratorResult ) )
            {
                this.Logger.Trace?.Log(
                    $"{this.GetType().Name}.Execute('{this.ProjectKey}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): generated sources did not change." );

                return false;
            }

            this.LastSourceGeneratorResult = newSourceGeneratorResult;

            this._observer?.OnGeneratedCodePublished(
                this.ProjectKey,
                newSourceGeneratorResult.AdditionalSources.ToImmutableDictionary( x => x.Key, x => x.Value.GeneratedSyntaxTree.ToString() ) );

            this.Logger.Trace?.Log(
                $"{this.GetType().Name}.Execute('{this.ProjectKey}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): {newSourceGeneratorResult.AdditionalSources.Count} source(s) generated. New digest: {newSourceGeneratorResult.GetDigest()}." );

            return true;
        }
        catch ( OperationCanceledException )
        {
            this.Logger.Warning?.Log(
                $"{this.GetType().Name}.Execute('{this.ProjectKey}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): cancelled." );

            throw;
        }
    }

    /// <summary>
    /// Executes the pipeline and then publishes the changes. 
    /// </summary>
    private async Task ComputeAndPublishAsync( Compilation compilation, TestableCancellationToken cancellationToken )
    {
        if ( await this.ComputeAsync( compilation, cancellationToken ) )
        {
            // From the moment the computation has completed, publishing cannot be cancelled.
            await this.PublishAsync();
        }
    }

    /// <summary>
    /// Publish the current cached content to the client (if implemented in the derived class) or
    /// by touching the touch file. 
    /// </summary>
    private async Task PublishAsync()
    {
        try
        {
            this.Logger.Trace?.Log( $"{this.GetType().Name}.Publish('{this.ProjectKey}')" );

            // Publish to the interactive process. We need to await before we change the touch file.
            await this.PublishGeneratedSourcesAsync( this.ProjectKey, CancellationToken.None );

            // Notify Roslyn that we have changes.
            if ( this.ProjectOptions.SourceGeneratorTouchFile == null )
            {
                this.Logger.Error?.Log( $"Property {MSBuildPropertyNames.MetalamaSourceGeneratorTouchFile} is undefined for project '{this.ProjectKey}'." );
            }
            else
            {
                // Note that we cannot cancel here. If we have published the source code, we must also touch the file.

                this.UpdateTouchFile();
            }

            this.Logger.Trace?.Log( $"{this.GetType().Name}.Publish('{this.ProjectKey}'): completed." );
        }
        catch ( OperationCanceledException )
        {
            this.Logger.Trace?.Log( $"{this.GetType().Name}.Publish('{this.ProjectKey}'): cancelled" );

            throw;
        }
    }

    protected void UpdateTouchFile()
    {
        var newGuid = Guid.NewGuid().ToString();

        // this.Logger.Trace?.Log( $"Touching '{this._sourceGeneratorTouchFile}' with value '{newGuid}'." );

        RetryHelper.Retry( () => File.WriteAllText( this._sourceGeneratorTouchFile!, newGuid ) );
    }

    /// <summary>
    /// When implemented by a derived class, publishes the generated source code to the client.
    /// </summary>
    protected virtual Task PublishGeneratedSourcesAsync( ProjectKey projectKey, CancellationToken cancellationToken ) => Task.CompletedTask;

    protected override void Dispose( bool disposing )
    {
        this._disposed = true;

        base.Dispose( disposing );
        this._currentCancellationSource?.Dispose();
    }
}