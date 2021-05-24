// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.DesignTime.Utilities;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
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
        // Syntax trees that may have compile time code based on namespaces. When a syntax tree is known to be compile-time but
        // has been invalidated, we don't remove it from the dictionary, but we set its value to null. It allows to know
        // that this specific tree is outdated, which then allows us to display a warning.
        private readonly ConcurrentDictionary<string, SyntaxTree?> _compileTimeSyntaxTrees = new();

        private readonly object _configureSync = new();
        private readonly CompilationChangeTracker _compilationChangeTracker = new();
        private readonly FileSystemWatcher? _fileSystemWatcher;

        private AspectPipelineConfiguration? _lastKnownConfiguration;

        public DesignTimeAspectPipelineStatus Status { get; private set; }

        // It's ok if we return an obsolete project in this case.
        public IReadOnlyList<AspectClass>? AspectClasses => this._lastKnownConfiguration?.AspectClasses;

        public DesignTimeAspectPipeline( IProjectOptions projectOptions, CompileTimeDomain domain ) : base( projectOptions, domain )
        {
            if ( projectOptions.BuildTouchFile != null )
            {
                this._fileSystemWatcher = new FileSystemWatcher(
                    Path.GetDirectoryName( projectOptions.BuildTouchFile ),
                    "*" + Path.GetExtension( projectOptions.BuildTouchFile ) ) { IncludeSubdirectories = true };

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
                DesignTimeLogger.Instance?.Write( $"Detected an external build for project '{this.ProjectOptions.AssemblyName}'." );

                var hasRelevantChange = false;

                foreach ( var file in this._compileTimeSyntaxTrees )
                {
                    if ( file.Value == null )
                    {
                        hasRelevantChange = true;
                        DesignTimeLogger.Instance?.Write( $"Touching file '{file.Key}'." );
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
            this.Reset();
            this.ExternalBuildStarted?.Invoke( this, EventArgs.Empty );
        }

        private IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees( Compilation compilation )
        {
            List<SyntaxTree> trees = new( this._compileTimeSyntaxTrees.Count );

            if ( this._compilationChangeTracker.LastCompilation == null )
            {
                lock ( this._configureSync )
                {
                    this._compileTimeSyntaxTrees.Clear();

                    this.InvalidateCache( compilation );
                }
            }
            else
            {
                foreach ( var syntaxTree in compilation.SyntaxTrees )
                {
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
            => this._compileTimeSyntaxTrees.TryGetValue( name, out var syntaxTree ) && syntaxTree == null;

        /// <summary>
        /// Invalidates the cache given a new <see cref="Compilation"/> and returns the set of changes between
        /// the previous compilation and the new one.
        /// </summary>
        public CompilationChange InvalidateCache( Compilation newCompilation )
        {
            lock ( this._configureSync )
            {
                var compilationChange = this._compilationChangeTracker.Update( newCompilation );

                if ( compilationChange.HasCompileTimeCodeChange )
                {
                    foreach ( var change in compilationChange.SyntaxTreeChanges )
                    {
                        switch ( change.CompileTimeChangeKind )
                        {
                            case CompileTimeChangeKind.None:
                                if ( change.HasCompileTimeCode )
                                {
                                    this._compileTimeSyntaxTrees[change.FilePath] = null;
                                    OnCompileTimeChange();
                                }

                                break;

                            case CompileTimeChangeKind.NewlyCompileTime:
                                this._compileTimeSyntaxTrees[change.FilePath] = change.NewTree;
                                OnCompileTimeChange();

                                break;

                            case CompileTimeChangeKind.NoLongerCompileTime:
                                this._compileTimeSyntaxTrees.TryRemove( change.FilePath, out _ );
                                OnCompileTimeChange();

                                break;
                        }
                    }
                }

                return compilationChange;
            }

            void OnCompileTimeChange()
            {
                if ( this.Status == DesignTimeAspectPipelineStatus.Ready )
                {
                    DesignTimeLogger.Instance?.Write(
                        $"DesignTimeAspectPipeline.InvalidateCache('{newCompilation.AssemblyName}'): compile-time change detected." );

                    this.Status = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                    if ( this.ProjectOptions.BuildTouchFile != null && File.Exists( this.ProjectOptions.BuildTouchFile ) )
                    {
                        using var mutex = MutexHelper.WithGlobalLock( this.ProjectOptions.BuildTouchFile );
                        File.Delete( this.ProjectOptions.BuildTouchFile );
                    }
                }
            }
        }

        /// <summary>
        /// Resets the current pipeline, including all caches and statuses.
        /// </summary>
        public void Reset()
        {
            lock ( this._configureSync )
            {
                DesignTimeLogger.Instance?.Write( $"DesignTimeAspectPipeline.Reset('{this.ProjectOptions.AssemblyName}')." );

                this._lastKnownConfiguration = null;
                this.Status = DesignTimeAspectPipelineStatus.Default;
                this._compilationChangeTracker.Reset();
                this._compileTimeSyntaxTrees.Clear();
            }
        }

        internal bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
        {
            lock ( this._configureSync )
            {
                if ( this._lastKnownConfiguration == null )
                {
                    // If we don't have any configuration, we will build one, because this is the first time we are called.

                    var compileTimeTrees = this.GetCompileTimeSyntaxTrees( compilation.Compilation );

                    if ( !this.TryInitialize(
                        diagnosticAdder,
                        compilation,
                        compileTimeTrees,
                        cancellationToken,
                        out configuration ) )
                    {
                        // A failure here means an error or a cache miss.

                        DesignTimeLogger.Instance?.Write( $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') failed." );

                        configuration = null;

                        return false;
                    }
                    else
                    {
                        DesignTimeLogger.Instance?.Write(
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

                    DesignTimeLogger.Instance?.Write(
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
            this.InvalidateCache( compilation.Compilation );

            if ( !this.TryGetConfiguration( compilation, diagnosticList, cancellationToken, out var configuration ) )
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

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new SourceGeneratorPipelineStage( compileTimeProject, parts, this );

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
        }
    }
}