// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    internal class DesignTimeAspectPipeline : AspectPipeline
    {
        private readonly object _configureSync = new();
        private readonly CompilationChangeTracker _compilationChangeTracker = new();
        private readonly FileSystemWatcher? _fileSystemWatcher;

        // Syntax trees that may have compile time code based on namespaces. When a syntax tree is known to be compile-time but
        // has been invalidated, we don't remove it from the dictionary, but we set its value to null. It allows to know
        // that this specific tree is outdated, which then allows us to display a warning.
        private ImmutableDictionary<string, SyntaxTree?>? _compileTimeSyntaxTrees;

        private AspectPipelineConfiguration? _lastKnownConfiguration;

        /// <summary>
        /// Gets an object that can be locked to get exclusive access to
        /// the current instance.
        /// </summary>
        public object Sync { get; } = new();

        public DesignTimeAspectPipelineStatus Status { get; private set; }

        // It's ok if we return an obsolete project in this case.
        public IReadOnlyList<AspectClass>? AspectClasses => this._lastKnownConfiguration?.AspectClasses;

        public DesignTimeAspectPipeline( IProjectOptions projectOptions, CompileTimeDomain domain, bool isTest )
            : base( projectOptions, domain, AspectExecutionScenario.DesignTime, isTest )
        {
            if ( projectOptions.BuildTouchFile != null )
            {
                var watchedFilter = "*" + Path.GetExtension( projectOptions.BuildTouchFile );
                var watchedDirectory = Path.GetDirectoryName( projectOptions.BuildTouchFile );

                this._fileSystemWatcher = new FileSystemWatcher( watchedDirectory, watchedFilter ) { IncludeSubdirectories = false };

                this._fileSystemWatcher.Changed += this.OnOutputDirectoryChanged;
                this._fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        public event EventHandler? ExternalBuildStarted;

        private void OnOutputDirectoryChanged( object sender, FileSystemEventArgs e )
        {
            if ( e.FullPath == this.ProjectOptions.BuildTouchFile &&
                 this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
            {
                // There was an external build. Touch the files to re-run the analyzer.
                Logger.Instance?.Write( $"Detected an external build for project '{this.ProjectOptions.AssemblyName}'." );

                var hasRelevantChange = false;

                foreach ( var file in this._compileTimeSyntaxTrees.AssertNotNull() )
                {
                    if ( file.Value == null )
                    {
                        hasRelevantChange = true;
                        Logger.Instance?.Write( $"Touching file '{file.Key}'." );
                        File.SetLastWriteTimeUtc( file.Key, DateTime.UtcNow );
                    }
                }

                if ( hasRelevantChange )
                {
                    this.OnExternalBuildStarted();
                }
            }
        }

        internal void OnExternalBuildStarted()
        {
            this._compilationChangeTracker.Reset();
            this.Reset();
            this.ExternalBuildStarted?.Invoke( this, EventArgs.Empty );
        }

        private IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees( Compilation compilation, CancellationToken cancellationToken )
        {
            List<SyntaxTree> trees = new( this._compileTimeSyntaxTrees?.Count ?? 8 );

            if ( this._compileTimeSyntaxTrees == null )
            {
                // The cache has not been set yet, so we need to compute the value from zero.

                if ( this._compilationChangeTracker.LastCompilation != null && this._compilationChangeTracker.LastCompilation != compilation )
                {
                    throw new AssertionFailedException();
                }

                var newCompileTimeSyntaxTrees = ImmutableDictionary<string, SyntaxTree?>.Empty;

                foreach ( var syntaxTree in compilation.SyntaxTrees )
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if ( CompileTimeCodeDetector.HasCompileTimeCode( syntaxTree.GetRoot() ) )
                    {
                        newCompileTimeSyntaxTrees = newCompileTimeSyntaxTrees.Add( syntaxTree.FilePath, syntaxTree );
                        trees.Add( syntaxTree );
                    }
                }

                this._compileTimeSyntaxTrees = newCompileTimeSyntaxTrees;
            }
            else
            {
                foreach ( var syntaxTree in compilation.SyntaxTrees )
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    if ( this._compileTimeSyntaxTrees.ContainsKey( syntaxTree.FilePath ) )
                    {
                        trees.Add( syntaxTree );
                    }
                }
            }

            return trees;
        }

        /// <summary>
        /// Determines whether a compile-time syntax tree is outdated. This happens when the syntax
        /// tree has changed compared to the cached configuration of this pipeline. This method is used to
        /// determine whether an error must displayed in the editor.  
        /// </summary>
        public bool IsCompileTimeSyntaxTreeOutdated( string name )
            => this._compileTimeSyntaxTrees.AssertNotNull().TryGetValue( name, out var syntaxTree ) && syntaxTree == null;

        /// <summary>
        /// Invalidates the cache given a new <see cref="Compilation"/> and returns the set of changes between
        /// the previous compilation and the new one.
        /// </summary>
        public CompilationChange InvalidateCache( Compilation newCompilation, CancellationToken cancellationToken )
        {
            lock ( this._configureSync )
            {
                var compilationChange = this._compilationChangeTracker.Update( newCompilation, cancellationToken );

                // Do not cancel in the middle of cache invalidation!

                if ( compilationChange.HasCompileTimeCodeChange )
                {
                    var newCompileTimeSyntaxTrees = this._compileTimeSyntaxTrees;

                    if ( newCompileTimeSyntaxTrees == null )
                    {
                        if ( compilationChange.IsIncremental )
                        {
                            throw new AssertionFailedException( "Got an incremental compilation change, but _compileTimeSyntaxTrees is null." );
                        }
                        else
                        {
                            newCompileTimeSyntaxTrees = ImmutableDictionary<string, SyntaxTree?>.Empty;
                        }
                    }

                    foreach ( var change in compilationChange.SyntaxTreeChanges )
                    {
                        switch ( change.CompileTimeChangeKind )
                        {
                            case CompileTimeChangeKind.None:
                                if ( change.HasCompileTimeCode )
                                {
                                    newCompileTimeSyntaxTrees = newCompileTimeSyntaxTrees.SetItem( change.FilePath, null );

                                    // We require an external build because we don't want to invalidate the pipeline configuration at
                                    // each keystroke.
                                    OnCompileTimeChange( true );
                                }

                                break;

                            case CompileTimeChangeKind.NewlyCompileTime:
                                newCompileTimeSyntaxTrees = newCompileTimeSyntaxTrees.SetItem( change.FilePath, change.NewTree );
                                OnCompileTimeChange( false );

                                break;

                            case CompileTimeChangeKind.NoLongerCompileTime:
                                newCompileTimeSyntaxTrees = newCompileTimeSyntaxTrees.Remove( change.FilePath );
                                OnCompileTimeChange( false );

                                break;
                        }
                    }

                    this._compileTimeSyntaxTrees = newCompileTimeSyntaxTrees;
                }

                return compilationChange;
            }

            void OnCompileTimeChange( bool requireExternalBuild )
            {
                if ( this.Status == DesignTimeAspectPipelineStatus.Ready )
                {
                    Logger.Instance?.Write( $"DesignTimeAspectPipeline.InvalidateCache('{newCompilation.AssemblyName}'): compile-time change detected." );

                    if ( requireExternalBuild )
                    {
                        this.Status = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                        if ( this.ProjectOptions.BuildTouchFile != null && File.Exists( this.ProjectOptions.BuildTouchFile ) )
                        {
                            using var mutex = MutexHelper.WithGlobalLock( this.ProjectOptions.BuildTouchFile );
                            File.Delete( this.ProjectOptions.BuildTouchFile );
                        }
                    }
                    else
                    {
                        this.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Resets the current pipeline, including all caches and statuses.
        /// </summary>
        private void Reset()
        {
            lock ( this._configureSync )
            {
                Logger.Instance?.Write( $"DesignTimeAspectPipeline.Reset('{this.ProjectOptions.AssemblyName}')." );

                this._lastKnownConfiguration = null;
                this.Status = DesignTimeAspectPipelineStatus.Default;
                this._compileTimeSyntaxTrees = null;

                // We don't reset the change tracker from here because the current method is called as a result of a change detected by the change tracker.
                // If we call Reset here, we would never get a stable cached configuration, it would always be invalidated.
            }
        }

        internal bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            bool ignoreStatus,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            lock ( this._configureSync )
            {
                if ( this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild && ignoreStatus )
                {
                    this.Status = DesignTimeAspectPipelineStatus.Default;
                }

                if ( this._lastKnownConfiguration == null )
                {
                    // If we don't have any configuration, we will build one, because this is the first time we are called.

                    var compileTimeTrees = this.GetCompileTimeSyntaxTrees( compilation.Compilation, cancellationToken );

                    if ( !this.TryInitialize(
                        diagnosticAdder,
                        compilation,
                        compileTimeTrees,
                        cancellationToken,
                        out configuration ) )
                    {
                        // A failure here means an error or a cache miss.

                        Logger.Instance?.Write( $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') failed." );

                        configuration = null;

                        return false;
                    }
                    else
                    {
                        Logger.Instance?.Write(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') succeeded with a new configuration." );

                        this._lastKnownConfiguration = configuration;
                        this.Status = DesignTimeAspectPipelineStatus.Ready;

                        return true;
                    }
                }
                else
                {
                    if ( this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
                    {
                        throw new InvalidOperationException();
                    }

                    // We have a valid configuration and it is not outdated.

                    Logger.Instance?.Write(
                        $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') returned existing configuration." );

                    configuration = this._lastKnownConfiguration;

                    return true;
                }
            }
        }

        /// <summary>
        /// Executes the pipeline.
        /// </summary>
        public DesignTimeAspectPipelineResult Execute( PartialCompilation compilation, CancellationToken cancellationToken )
        {
            DiagnosticList diagnosticList = new();

            if ( this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
            {
                throw new InvalidOperationException();
            }

            // The production use case should call UpdateCompilation before calling Execute, so a call to UpdateCompilation is redundant,
            // but some tests don't. Redundant calls to UpdateCompilation have no adverse side effect.
            this.InvalidateCache( compilation.Compilation, cancellationToken );

            if ( !this.TryGetConfiguration( compilation, diagnosticList, false, cancellationToken, out var configuration ) )
            {
                return new DesignTimeAspectPipelineResult(
                    false,
                    compilation.SyntaxTrees,
                    ImmutableArray<IntroducedSyntaxTree>.Empty,
                    new ImmutableUserDiagnosticList( diagnosticList ) );
            }

            var success = this.TryExecute( compilation, diagnosticList, configuration, cancellationToken, out var pipelineResult );

            var result = new DesignTimeAspectPipelineResult(
                success,
                compilation.SyntaxTrees,
                pipelineResult?.AdditionalSyntaxTrees ?? Array.Empty<IntroducedSyntaxTree>(),
                new ImmutableUserDiagnosticList(
                    diagnosticList.ToImmutableArray(),
                    pipelineResult?.Diagnostics.DiagnosticSuppressions ) );

            UserDiagnosticRegistrationService.GetInstance().RegisterDescriptors( result );

            return result;
        }

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new SourceGeneratorPipelineStage( compileTimeProject, parts, this.ServiceProvider );

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
        }
    }
}