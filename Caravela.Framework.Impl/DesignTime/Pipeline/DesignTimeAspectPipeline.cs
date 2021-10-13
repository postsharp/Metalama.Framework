// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
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
    public partial class DesignTimeAspectPipeline : AspectPipeline
    {
        private readonly IFileSystemWatcher? _fileSystemWatcher;

        private PipelineState _currentState;

        /// <summary>
        /// Gets an object that can be locked to get exclusive access to
        /// the current instance.
        /// </summary>
        public object Sync { get; } = new();

        internal DesignTimeAspectPipelineStatus Status => this._currentState.Status;

        // It's ok if we return an obsolete project in the use cases of this property.
        internal IEnumerable<AspectClass>? AspectClasses => this._currentState.Configuration?.AspectClasses.OfType<AspectClass>();

        public DesignTimeAspectPipeline(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            bool isTest )
            : base( serviceProvider.WithProjectScopedServices(), AspectExecutionScenario.DesignTime, isTest, domain )
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
            if ( e.FullPath == this.ProjectOptions.BuildTouchFile &&
                 this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
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
            this._currentState = new PipelineState( this );
            this.ExternalBuildStarted?.Invoke( this, EventArgs.Empty );
        }

        internal bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            bool ignoreStatus,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out AspectProjectConfiguration? configuration )
        {
            lock ( this.Sync )
            {
                var state = this._currentState;
                var success = PipelineState.TryGetConfiguration( ref state, compilation, diagnosticAdder, ignoreStatus, cancellationToken, out configuration );

                this._currentState = state;

                return success;
            }
        }

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject )
            => new SourceGeneratorPipelineStage( compileTimeProject, parts, this.ServiceProvider );

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
            this._fileSystemWatcher?.Dispose();
        }

        internal CompilationChanges InvalidateCache( Compilation compilation, CancellationToken cancellationToken )
        {
            lock ( this.Sync )
            {
                this._currentState = this._currentState.InvalidateCache( compilation, cancellationToken );

                return this._currentState.UnprocessedChanges.AssertNotNull();
            }
        }

        public DesignTimeAspectPipelineResult Execute( PartialCompilation partialCompilation, CancellationToken cancellationToken )
        {
            var state = this._currentState;
            var result = PipelineState.Execute( ref state, partialCompilation, cancellationToken );

            // Intentionally updating the state atomically after successful execution of the method, so the state is
            // not affected by a cancellation.
            this._currentState = state;

            return result;
        }

        /// <summary>
        /// Determines whether a compile-time syntax tree is outdated. This happens when the syntax
        /// tree has changed compared to the cached configuration of this pipeline. This method is used to
        /// determine whether an error must displayed in the editor.  
        /// </summary>
        public bool IsCompileTimeSyntaxTreeOutdated( string name )
            => this._currentState.CompileTimeSyntaxTrees.AssertNotNull().TryGetValue( name, out var syntaxTree ) && syntaxTree == null;
    }
}