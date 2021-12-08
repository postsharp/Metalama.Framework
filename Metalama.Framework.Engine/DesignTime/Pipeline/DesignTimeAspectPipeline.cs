// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.DesignTime.Diff;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Pipeline
{
    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    /// Must be public because of testing.
    public partial class DesignTimeAspectPipeline : AspectPipeline
    {
        private readonly ConditionalWeakTable<Compilation, CompilationResult> _compilationResultCache = new();
        private static readonly string _sourceGeneratorAssemblyName = typeof(DesignTimeAspectPipelineFactory).Assembly.GetName().Name;

        private readonly IFileSystemWatcher? _fileSystemWatcher;

        private PipelineState _currentState;
        private int _pipelineExecutionCount;

        /// <summary>
        /// Gets the number of times the pipeline has been executed. Useful for testing purposes.
        /// </summary>
        public int PipelineExecutionCount => this._pipelineExecutionCount;

        /// <summary>
        /// Gets an object that can be locked to get exclusive access to
        /// the current instance.
        /// </summary>
        private readonly object _sync = new();

        // ReSharper disable once InconsistentlySynchronizedField
        internal DesignTimeAspectPipelineStatus Status => this._currentState.Status;

        // It's ok if we return an obsolete project in the use cases of this property.
        // ReSharper disable once InconsistentlySynchronizedField
        internal IEnumerable<AspectClass>? AspectClasses => this._currentState.Configuration?.AspectClasses.OfType<AspectClass>();

        public DesignTimeAspectPipeline(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            IEnumerable<MetadataReference> metadataReferences,
            bool isTest )
            : base( serviceProvider.WithProjectScopedServices( metadataReferences ), ExecutionScenario.DesignTime, isTest, domain )
        {
            this._currentState = new PipelineState( this );

            // The design-time pipeline contains project-scoped services for performance reasons: the pipeline may be called several
            // times with the same compilation.

            if ( this.ProjectOptions.BuildTouchFile == null )
            {
                return;
            }

            var watchedFilter = "*" + Path.GetExtension( this.ProjectOptions.BuildTouchFile );
            var watchedDirectory = Path.GetDirectoryName( this.ProjectOptions.BuildTouchFile );

            if ( watchedDirectory == null )
            {
                return;
            }

            var fileSystemWatcherFactory = this.ServiceProvider.GetOptionalService<IFileSystemWatcherFactory>() ?? new FileSystemWatcherFactory();
            this._fileSystemWatcher = fileSystemWatcherFactory.Create( watchedDirectory, watchedFilter );
            this._fileSystemWatcher.IncludeSubdirectories = false;

            this._fileSystemWatcher.Changed += this.OnOutputDirectoryChanged;
            this._fileSystemWatcher.EnableRaisingEvents = true;
        }

        public event EventHandler? ExternalBuildStarted;

        private void OnOutputDirectoryChanged( object sender, FileSystemEventArgs e )
        {
            if ( e.FullPath != this.ProjectOptions.BuildTouchFile || this.Status != DesignTimeAspectPipelineStatus.NeedsExternalBuild )
            {
                return;
            }

            lock ( this._sync )
            {
                // There was an external build. Touch the files to re-run the analyzer.
                Logger.Instance?.Write( $"Detected an external build for project '{this.ProjectOptions.AssemblyName}'." );

                var hasRelevantChange = false;

                foreach ( var file in this._currentState.CompileTimeSyntaxTrees.AssertNotNull() )
                {
                    if ( file.Value == null )
                    {
                        hasRelevantChange = true;
                        Logger.Instance?.Write( $"Touching file '{file.Key}'." );
                        RetryHelper.Retry( () => File.SetLastWriteTimeUtc( file.Key, DateTime.UtcNow ) );
                    }
                }

                if ( hasRelevantChange )
                {
                    this.ExternalBuildStarted?.Invoke( this, EventArgs.Empty );

                    this._currentState = new PipelineState( this );
                }
            }
        }

        public void OnExternalBuildStarted()
        {
            lock ( this._sync )
            {
                this._currentState = new PipelineState( this );
                this.ExternalBuildStarted?.Invoke( this, EventArgs.Empty );
            }
        }

        internal bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            bool ignoreStatus,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            lock ( this._sync )
            {
                var state = this._currentState;
                var success = PipelineState.TryGetConfiguration( ref state, compilation, diagnosticAdder, ignoreStatus, cancellationToken, out configuration );

                this._currentState = state;

                return success;
            }
        }

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new SourceGeneratorPipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );

        private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
            => null;

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
        }

        private CompilationChanges InvalidateCache( Compilation compilation, bool invalidateCompilationResult, CancellationToken cancellationToken )
        {
            this._currentState = this._currentState.InvalidateCacheForNewCompilation( compilation, invalidateCompilationResult, cancellationToken );

            return this._currentState.UnprocessedChanges.AssertNotNull();
        }

        public void InvalidateCache()
        {
            lock ( this._sync )
            {
                this._currentState = this._currentState.Reset();
            }
        }

        private bool TryExecutePartial( PartialCompilation partialCompilation, CancellationToken cancellationToken, out CompilationResult? compilationResult )
        {
            var state = this._currentState;

            if ( !PipelineState.TryExecute( ref state, partialCompilation, cancellationToken, out compilationResult ) )
            {
                return false;
            }

            // Intentionally updating the state atomically after successful execution of the method, so the state is
            // not affected by a cancellation.
            this._currentState = state;

            return true;
        }

        public bool TryExecute( Compilation compilation, CancellationToken cancellationToken, [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
            {
                return true;
            }

            lock ( this._sync )
            {
                if ( this._compilationResultCache.TryGetValue( compilation, out compilationResult ) )
                {
                    return true;
                }

                // Invalidate the cache for the new compilation.
                var changes = this.InvalidateCache( compilation, this.Status != DesignTimeAspectPipelineStatus.NeedsExternalBuild, cancellationToken );

                var compilationToAnalyze = changes.CompilationToAnalyze;

                if ( this.Status != DesignTimeAspectPipelineStatus.NeedsExternalBuild )
                {
                    var dirtySyntaxTrees = this.GetDirtySyntaxTrees( compilationToAnalyze );

                    // Execute the pipeline if required, and update the cache.
                    if ( dirtySyntaxTrees.Count > 0 )
                    {
                        var partialCompilation = PartialCompilation.CreatePartial( compilationToAnalyze, dirtySyntaxTrees );

                        if ( !partialCompilation.IsEmpty )
                        {
                            Interlocked.Increment( ref this._pipelineExecutionCount );

                            if ( !this.TryExecutePartial( partialCompilation, cancellationToken, out compilationResult ) )
                            {
                                return false;
                            }
                        }
                    }

                    compilationResult = this._currentState.CompilationResult;

                    this._compilationResultCache.Add( compilation, compilationResult );

                    return true;
                }
                else
                {
                    // If we need a build, we only serve results from the cache.
                    Logger.Instance?.Write(
                        $"DesignTimeAspectPipelineCache.TryExecute('{compilation.AssemblyName}'): build required," +
                        $" returning from cache only." );

                    compilationResult = this._currentState.CompilationResult;

                    return true;
                }
            }
        }

        private List<SyntaxTree> GetDirtySyntaxTrees( Compilation compilation )
        {
            // Computes the set of semantic models that need to be processed.

            List<SyntaxTree> uncachedSyntaxTrees = new();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                if ( syntaxTree.FilePath.StartsWith( _sourceGeneratorAssemblyName, StringComparison.Ordinal ) )
                {
                    // This is our own generated file. Don't include.
                    continue;
                }

                if ( this._currentState.CompilationResult.IsSyntaxTreeDirty( syntaxTree ) )
                {
                    uncachedSyntaxTrees.Add( syntaxTree );
                }
            }

            return uncachedSyntaxTrees;
        }

        /// <summary>
        /// Determines whether a compile-time syntax tree is outdated. This happens when the syntax
        /// tree has changed compared to the cached configuration of this pipeline. This method is used to
        /// determine whether an error must displayed in the editor.  
        /// </summary>
        public bool IsCompileTimeSyntaxTreeOutdated( string name )
            => this._currentState.CompileTimeSyntaxTrees.AssertNotNull().TryGetValue( name, out var syntaxTree ) && syntaxTree == null;

        internal IEnumerable<AspectClass> GetEligibleAspects( Compilation compilation, ISymbol symbol, CancellationToken cancellationToken )
        {
            var classes = this.AspectClasses;

            if ( classes == null )
            {
                yield break;
            }

            IDeclaration? declaration = null;

            foreach ( var aspectClass in classes )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( !aspectClass.IsAbstract && aspectClass.IsEligibleFast( symbol ) )
                {
                    // We have a candidate. Create an IDeclaration if we haven't done it yet.
                    if ( declaration == null )
                    {
                        var projectModel = new ProjectModel( compilation, this.ServiceProvider.WithMark( ServiceProviderMark.Project ) );

                        var compilationModel = CompilationModel.CreateInitialInstance(
                            projectModel,
                            PartialCompilation.CreatePartial( compilation, Array.Empty<SyntaxTree>() ) );

                        declaration = compilationModel.Factory.GetDeclaration( symbol );
                    }

                    var eligibleScenarios = aspectClass.GetEligibility( declaration );

                    if ( eligibleScenarios.IncludesAny( EligibleScenarios.All ) )
                    {
                        yield return aspectClass;
                    }
                }
            }
        }
    }
}