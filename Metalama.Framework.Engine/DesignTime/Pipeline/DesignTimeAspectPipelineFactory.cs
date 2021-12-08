// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.DesignTime.Diff;
using Metalama.Framework.Engine.DesignTime.Refactoring;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

// ReSharper disable InconsistentlySynchronizedField

namespace Metalama.Framework.Engine.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
    /// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
    /// cache invalidation methods as appropriate.
    /// </summary>
    internal class DesignTimeAspectPipelineFactory : IDisposable, IInheritableAspectManifestProvider
    {
        // The project id is passed to a constant, because that's the only public way to push a property to a compilation.
        public const string ProjectIdPreprocessorSymbolPrefix = "MetalamaProjectId_";

        private readonly ConcurrentDictionary<string, DesignTimeAspectPipeline> _pipelinesByProjectId = new();

        public CompileTimeDomain Domain { get; }

        private readonly bool _isTest;

        public DesignTimeAspectPipelineFactory( CompileTimeDomain domain, bool isTest = false )
        {
            this.Domain = domain;
            this._isTest = isTest;
        }

        /// <summary>
        /// Gets the singleton instance of this class (other instances can be used in tests).
        /// </summary>
        public static DesignTimeAspectPipelineFactory Instance { get; } = new( new CompileTimeDomain() );

        protected virtual string GetProjectId( IProjectOptions projectOptions, Compilation compilation ) => projectOptions.ProjectId;

        /// <summary>
        /// Gets the pipeline for a given project, and creates it if necessary.
        /// </summary>
        /// <param name="projectOptions"></param>
        /// <returns></returns>
        internal DesignTimeAspectPipeline? GetOrCreatePipeline( IProjectOptions projectOptions, Compilation compilation, CancellationToken cancellationToken )
        {
            if ( !projectOptions.IsFrameworkEnabled )
            {
                return null;
            }

            // We lock the dictionary because the ConcurrentDictionary does not guarantee that the creation delegate
            // is called only once, and we prefer a single instance for the simplicity of debugging. 

            var projectId = this.GetProjectId( projectOptions, compilation );

            if ( this._pipelinesByProjectId.TryGetValue( projectId, out var pipeline ) )
            {
                // TODO: we must validate that the project options and metadata references are still identical to those cached, otherwise we should create a new pipeline.
                return pipeline;
            }
            else
            {
                lock ( this._pipelinesByProjectId )
                {
                    if ( this._pipelinesByProjectId.TryGetValue( projectId, out pipeline ) )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        return pipeline;
                    }

                    var serviceProvider = ServiceProviderFactory.GetServiceProvider().WithServices( projectOptions, this );
                    pipeline = new DesignTimeAspectPipeline( serviceProvider, this.Domain, compilation.References, this._isTest );
                    pipeline.ExternalBuildStarted += this.OnExternalBuildStarted;

                    if ( !this._isTest )
                    {
                        // We _intentionally_ wait 5 seconds before starting a pipeline. This allows the initial burst of requests
                        // to "settle down" and hopefully only the final will survive.
                        // This is a temporary solution until we understand that happens.
                        // TODO #29089
                        Thread.Sleep( 5000 );
                    }

                    if ( !this._pipelinesByProjectId.TryAdd( projectId, pipeline ) )
                    {
                        throw new AssertionFailedException();
                    }

                    return pipeline;
                }
            }
        }

        private void OnExternalBuildStarted( object sender, EventArgs e )
        {
            // If a build has started, we have to invalidate the whole cache because we have allowed
            // our cache to become inconsistent when we started to have an outdated pipeline configuration.
            foreach ( var pipeline in this._pipelinesByProjectId.Values )
            {
                pipeline.InvalidateCache();
            }
        }

        public IEnumerable<AspectClass> GetEligibleAspects(
            Compilation compilation,
            ISymbol symbol,
            IProjectOptions projectOptions,
            CancellationToken cancellationToken )
        {
            var pipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( pipeline == null )
            {
                return Enumerable.Empty<AspectClass>();
            }

            return pipeline.GetEligibleAspects( compilation, symbol, cancellationToken );
        }

        public bool TryExecute(
            IProjectOptions projectOptions,
            Compilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out CompilationResult? compilationResult )
        {
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions, compilation, cancellationToken );

            if ( designTimePipeline == null )
            {
                compilationResult = null;

                return false;
            }

            return designTimePipeline.TryExecute( compilation, cancellationToken, out compilationResult );
        }

        public bool TryApplyAspectToCode(
            IProjectOptions projectOptions,
            AspectClass aspectClass,
            IAspect aspect,
            Compilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions, inputCompilation, cancellationToken );

            if ( designTimePipeline == null )
            {
                outputCompilation = null;
                diagnostics = ImmutableArray<Diagnostic>.Empty;

                return false;
            }

            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = inputCompilation.SyntaxTrees.Where( CompilationChangeTracker.IsGeneratedFile );
            var sourceCompilation = inputCompilation.RemoveSyntaxTrees( generatedFiles );

            var sourceSymbol = DocumentationCommentId
                .GetFirstSymbolForDeclarationId( targetSymbol.GetDocumentationCommentId().AssertNotNull(), sourceCompilation )
                .AssertNotNull();

            // TODO: use partial compilation (it does not seem to work).
            var partialCompilation = PartialCompilation.CreateComplete( sourceCompilation );

            DiagnosticList configurationDiagnostics = new();

            if ( !designTimePipeline.TryGetConfiguration( partialCompilation, configurationDiagnostics, true, cancellationToken, out var configuration ) )
            {
                outputCompilation = null;
                diagnostics = configurationDiagnostics.ToImmutableArray();

                return false;
            }

            return LiveTemplateAspectPipeline.TryExecute(
                configuration.ServiceProvider,
                this.Domain,
                configuration,
                aspectClass,
                aspect,
                partialCompilation,
                sourceSymbol,
                cancellationToken,
                out outputCompilation,
                out diagnostics );
        }

        public void Dispose()
        {
            foreach ( var designTimeAspectPipeline in this._pipelinesByProjectId.Values )
            {
                designTimeAspectPipeline.Dispose();
            }

            this._pipelinesByProjectId.Clear();
            this.Domain.Dispose();
        }

        public virtual bool TryGetPipeline( Compilation compilation, [NotNullWhen( true )] out DesignTimeAspectPipeline? pipeline )
        {
            var projectIdConstant = compilation.SyntaxTrees.First()
                .Options.PreprocessorSymbolNames.FirstOrDefault( x => x.StartsWith( ProjectIdPreprocessorSymbolPrefix, StringComparison.OrdinalIgnoreCase ) );

            var projectId = projectIdConstant?.Substring( ProjectIdPreprocessorSymbolPrefix.Length );

            if ( projectId == null )
            {
                // The compilation does not reference our package.
                pipeline = null;

                return false;
            }

            return this._pipelinesByProjectId.TryGetValue( projectId, out pipeline );
        }

        public IInheritableAspectsManifest? GetInheritableAspectsManifest( Compilation compilation, CancellationToken cancellationToken )
        {
            if ( !this.TryGetPipeline( compilation, out var pipeline ) )
            {
                // We cannot create the pipeline because we don't have all options.
                // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

                return null;
            }

            if ( !pipeline.TryExecute( compilation, cancellationToken, out var compilationResult ) )
            {
                return null;
            }

            return compilationResult;
        }
    }
}