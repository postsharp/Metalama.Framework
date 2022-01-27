// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.DesignTime;
using Metalama.Framework.Engine.DesignTime.Pipeline;
using Metalama.Framework.Engine.DesignTime.Remoting;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.VisualStudio;

public class AnalysisProcessSourceGenerator : DesignTimeSourceGenerator
{
    private ServiceHost? _serviceHost;

    public AnalysisProcessSourceGenerator()
    {
        if ( ServiceHost.TryGetPipeName( out var pipeName ) )
        {
            this._serviceHost = new ServiceHost( pipeName );
            this._serviceHost.Start();
        }
    }
    
    private class AnalysisProcessSourceGeneratorImpl : SourceGeneratorImpl
    {
        private readonly ServiceHost? _serviceHost;
        private CancellationTokenSource? _currentCancellationSource;
        private ImmutableDictionary<string, SourceText> _currentSources = ImmutableDictionary<string, SourceText>.Empty;
        private ImmutableDictionary<string, IntroducedSyntaxTree>? _lastIntroducedTrees;

        public AnalysisProcessSourceGeneratorImpl( ServiceHost? serviceHost )
        {
            this._serviceHost = serviceHost;
        }

        public override void GenerateSources( IProjectOptions projectOptions, Compilation compilation, GeneratorExecutionContext context )
        {
            // Serve from the cache.
            foreach ( var source in this._currentSources )
            {
                context.AddSource( source.Key, source.Value );
            }

            // Cancel the previous computation.
            this._currentCancellationSource?.Cancel();
            var cancellationSource = this._currentCancellationSource = new CancellationTokenSource();
            _ = Task.Run( () => this.ComputeAsync( projectOptions, compilation, cancellationSource.Token ), cancellationSource.Token );
        }

        private async Task ComputeAsync( IProjectOptions projectOptions, Compilation compilation, CancellationToken cancellationToken )
        {
            // Execute the pipeline.
            if ( !DesignTimeAspectPipelineFactory.Instance.TryExecute(
                    projectOptions,
                    compilation,
                    cancellationToken,
                    out var compilationResult ) )
            {
                Logger.DesignTime.Trace?.Log(
                    $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): the pipeline failed." );

                Logger.DesignTime.Trace?.Log(
                    " Compilation references: " + string.Join(
                        ", ",
                        compilation.References.GroupBy( r => r.GetType() ).Select( g => $"{g.Key.Name}: {g.Count()}" ) ) );

                return;
            }

            // Check if the pipeline returned any difference. If not, do not update our cache.
            if ( compilationResult.PipelineResult.IntroducedSyntaxTrees != this._lastIntroducedTrees )
            {
                Logger.DesignTime.Trace?.Log(
                    $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): generated sources did not change." );

                return;
            }

            this._lastIntroducedTrees = compilationResult.PipelineResult.IntroducedSyntaxTrees;

            // Cache the introduces trees.
            var generatedSources = ImmutableDictionary.CreateBuilder<string, SourceText>();

            foreach ( var introducedSyntaxTree in compilationResult.PipelineResult.IntroducedSyntaxTrees )
            {
                Logger.DesignTime.Trace?.Log( $"  AddSource('{introducedSyntaxTree.Key}')" );
                var sourceText = await introducedSyntaxTree.Value.GeneratedSyntaxTree.GetTextAsync( cancellationToken );
                generatedSources[introducedSyntaxTree.Value.Name] = sourceText;
            }

            this._currentSources = generatedSources.ToImmutable();

            Logger.DesignTime.Trace?.Log(
                $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): {generatedSources.Count} sources generated." );

            // Publish to the interactive process. We need to await before we change the touch file.
            await this._serviceHost.PublishGeneratedSourcesAsync(
                projectOptions.ProjectId,
                generatedSources.ToImmutableDictionary( x => x.Key, x => x.Value.ToString() ),
                cancellationToken );

            // Notify Roslyn that we have changes.
            if ( projectOptions.SourceGeneratorTouchFile == null )
            {
                Logger.DesignTime.Error?.Log( "Property MetalamaSourceGeneratorTouchFile cannot be null." );
            }
            else
            {
                RetryHelper.Retry( () => File.WriteAllText( projectOptions.SourceGeneratorTouchFile, Guid.NewGuid().ToString() ) );
            }
        }
    }

    protected override SourceGeneratorImpl CreateSourceGeneratorImpl() => new AnalysisProcessSourceGeneratorImpl( this._serviceHost );
}