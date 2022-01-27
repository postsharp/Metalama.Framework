// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime;

public class AnalysisProcessSourceGenerator : DesignTimeSourceGenerator
{
#pragma warning disable CA1001 // ServiceHost is disposable but not owned.
    protected class AnalysisProcessSourceGeneratorImpl : SourceGeneratorImpl
#pragma warning restore CA1001
    {
        private CancellationTokenSource? _currentCancellationSource;

        public ImmutableDictionary<string, SourceText> Sources { get; private set; } = ImmutableDictionary<string, SourceText>.Empty;

        private ImmutableDictionary<string, IntroducedSyntaxTree>? _lastIntroducedTrees;

        public override void GenerateSources( IProjectOptions projectOptions, Compilation compilation, GeneratorExecutionContext context )
        {
            // Serve from the cache.
            foreach ( var source in this.Sources )
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

            this.Sources = generatedSources.ToImmutable();

            Logger.DesignTime.Trace?.Log(
                $"DesignTimeSourceGenerator.Execute('{compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation )}): {generatedSources.Count} sources generated." );

            // Publish to the interactive process. We need to await before we change the touch file.
            await this.PublishGeneratedSourcesAsync( projectOptions.ProjectId, cancellationToken );

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

        protected virtual Task PublishGeneratedSourcesAsync( string projectId, CancellationToken cancellationToken ) => Task.CompletedTask;
    }

    protected override SourceGeneratorImpl CreateSourceGeneratorImpl() => new AnalysisProcessSourceGeneratorImpl();
}