// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The base class for the main process of Caravela.
    /// </summary>
    public abstract partial class AspectPipeline : IDisposable, IAspectPipelineProperties
    {
        public IBuildOptions BuildOptions { get; }

        private readonly CompileTimeDomain _domain;

        protected ServiceProvider ServiceProvider { get; } = new();

        IServiceProvider IAspectPipelineProperties.ServiceProvider => this.ServiceProvider;

        protected AspectPipeline( IBuildOptions buildOptions, CompileTimeDomain domain, IAssemblyLocator? assemblyLocator = null )
        {
            this._domain = domain;
            this.BuildOptions = buildOptions;
            this.ServiceProvider.AddService( buildOptions );
            this.ServiceProvider.AddService( ReferenceAssemblyLocator.GetInstance() );
            this.ServiceProvider.AddService( new SyntaxSerializationService() );

            if ( assemblyLocator != null )
            {
                this.ServiceProvider.AddService( assemblyLocator );
            }
        }

        /// <summary>
        /// Handles an exception thrown in the pipeline.
        /// </summary>
        /// <param name="exception"></param>
        protected void HandleException( Exception exception, IDiagnosticAdder diagnosticAdder )
        {
            switch ( exception )
            {
                case InvalidUserCodeException diagnosticsException:
                    foreach ( var diagnostic in diagnosticsException.Diagnostics )
                    {
                        diagnosticAdder.Report( diagnostic );
                    }

                    break;

                default:
                    if ( this.WriteUnhandledExceptionsToFile )
                    {
                        var guid = Guid.NewGuid();
                        var crashReportDirectory = this.BuildOptions.GetCrashReportDirectoryOrDefault();

                        if ( string.IsNullOrWhiteSpace( crashReportDirectory ) )
                        {
                            crashReportDirectory = Path.GetTempPath();
                        }

                        if ( !Directory.Exists( crashReportDirectory ) )
                        {
                            Directory.CreateDirectory( crashReportDirectory );
                        }

                        var path = Path.Combine( crashReportDirectory, $"caravela-{exception.GetType().Name}-{guid}.txt" );

                        try
                        {
                            File.WriteAllText( path, exception.ToString() );
                        }
                        catch ( IOException ) { }

                        Console.WriteLine( exception.ToString() );

                        diagnosticAdder.Report(
                            GeneralDiagnosticDescriptors.UncaughtException.CreateDiagnostic( null, (exception.ToDiagnosticString(), path) ) );
                    }

                    break;
            }
        }

        public virtual bool WriteUnhandledExceptionsToFile => true;

        public bool HasCachedCompileTimeProject( Compilation compilation, IDiagnosticAdder diagnosticAdder, IReadOnlyList<SyntaxTree>? compileTimeTreesHint )
        {
            var loader = CompileTimeProjectLoader.Create( this._domain, this.ServiceProvider );

            return loader.TryGetCompileTimeProject( compilation, compileTimeTreesHint, diagnosticAdder, true, out _ );
        }

        public int PipelineInitializationCount { get; set; }

        private protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            IReadOnlyList<SyntaxTree>? compileTimeTreesHint,
            [NotNullWhen( true )] out PipelineConfiguration? configuration )
        {
            this.PipelineInitializationCount++;

            var roslynCompilation = compilation.Compilation;

            // Create dependencies.
            var loader = CompileTimeProjectLoader.Create( this._domain, this.ServiceProvider );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProject( roslynCompilation, compileTimeTreesHint, diagnosticAdder, false, out var compileTimeProject ) )
            {
                configuration = null;

                return false;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, this.BuildOptions.PlugIns );
            var aspectTypeFactory = new AspectClassMetadataFactory( driverFactory );

            var aspectTypes = aspectTypeFactory.GetAspectClasses( compilation.Compilation, compileTimeProject, diagnosticAdder ).ToImmutableArray();

            // Get aspect parts and sort them.
            var unsortedAspectLayers = aspectTypes
                .Where( t => !t.IsAbstract )
                .SelectMany( at => at.Layers )
                .ToImmutableArray();

            var aspectOrderSources = new IAspectOrderingSource[]
            {
                new AttributeAspectOrderingSource( compilation.Compilation ), new AspectLayerOrderingSource( aspectTypes )
            };

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var sortedAspectLayers ) )
            {
                configuration = null;

                return false;
            }

            var aspectLayers = sortedAspectLayers.ToImmutableArray();

            var stages = aspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectClass.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), loader ) )
                .ToImmutableArray();

            configuration = new PipelineConfiguration( stages, aspectTypes, sortedAspectLayers, compileTimeProject, loader );

            return true;

            static object GetGroupingKey( IAspectDriver driver )
                => driver switch
                {
                    // weavers are not grouped together
                    // Note: this requires that every AspectType has its own instance of IAspectWeaver
                    IAspectWeaver weaver => weaver,

                    // AspectDrivers are grouped together
                    AspectDriver => nameof(AspectDriver),

                    _ => throw new NotSupportedException()
                };
        }

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        private protected static bool TryExecuteCore(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            PipelineConfiguration pipelineConfiguration,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            // If there is no aspect in the compilation, don't execute the pipeline.
            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.AspectClasses.Count == 0 )
            {
                pipelineStageResult = new PipelineStageResult( compilation, Array.Empty<OrderedAspectLayer>() );

                return true;
            }

            var aspectSource = new CompilationAspectSource(
                pipelineConfiguration.AspectClasses,
                pipelineConfiguration.CompileTimeProjectLoader );

            pipelineStageResult = new PipelineStageResult( compilation, pipelineConfiguration.Layers, aspectSources: new[] { aspectSource } );

            foreach ( var stage in pipelineConfiguration.Stages )
            {
                if ( !stage.TryExecute( pipelineStageResult!, diagnosticAdder, out var newStageResult ) )
                {
                    return false;
                }
                else
                {
                    pipelineStageResult = newStageResult;
                }
            }

            var hasError = pipelineStageResult.Diagnostics.ReportedDiagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error );

            foreach ( var diagnostic in pipelineStageResult.Diagnostics.ReportedDiagnostics )
            {
                diagnosticAdder.Report( diagnostic );
            }

            return !hasError;
        }

        /// <summary>
        /// Creates an instance of <see cref="HighLevelPipelineStage"/>.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="compileTimeProjectLoader"></param>
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader );

        private PipelineStage CreateStage(
            object groupKey,
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectClass, this );

                case nameof(AspectDriver):

                    return this.CreateStage( parts, compileTimeProjectLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        protected virtual void Dispose( bool disposing ) { }

        /// <inheritdoc/>
        public void Dispose() => this.Dispose( true );

        public abstract bool CanTransformCompilation { get; }
    }
}