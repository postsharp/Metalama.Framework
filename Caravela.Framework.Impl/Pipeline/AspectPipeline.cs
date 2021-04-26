// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
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
    public abstract class AspectPipeline : IDisposable, IAspectPipelineProperties
    {
        public IBuildOptions BuildOptions { get; }

        private readonly CompileTimeDomain _domain = new();

        protected ServiceProvider ServiceProvider { get; } = new();

        protected AspectPipeline( IBuildOptions buildOptions )
        {
            this.BuildOptions = buildOptions;
            this.ServiceProvider.AddService( buildOptions );
            this.ServiceProvider.AddService( new ReferenceAssemblyLocator() );
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
                        diagnosticAdder.ReportDiagnostic( diagnostic );
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

                        diagnosticAdder.ReportDiagnostic(
                            GeneralDiagnosticDescriptors.UncaughtException.CreateDiagnostic( null, (exception.ToDiagnosticString(), path) ) );
                    }

                    break;
            }
        }

        public virtual bool WriteUnhandledExceptionsToFile => true;

        private protected record PipelineConfiguration(
            ImmutableArray<PipelineStage> Stages,
            IReadOnlyList<AspectType> AspectTypes,
            ImmutableArray<OrderedAspectLayer> Layers,
            CompileTimeProject? CompileTimeProject,
            CompileTimeAssemblyLoader CompileTimeAssemblyLoader );

        private protected bool TryInitialize(
            IDiagnosticAdder diagnosticAdder,
            PartialCompilation compilation,
            [NotNullWhen( true )] out PipelineConfiguration? configuration )
        {
            var roslynCompilation = compilation.Compilation;

            // Create dependencies.
            var loader = CompileTimeAssemblyLoader.Create( this._domain, this.ServiceProvider, roslynCompilation );

            // Prepare the compile-time assembly.
            if ( !loader.TryGetCompileTimeProject( roslynCompilation.Assembly, diagnosticAdder, out var compileTimeProject ) )
            {
                configuration = null;

                return false;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation.Compilation, this.BuildOptions.PlugIns );
            var aspectTypeFactory = new AspectTypeFactory( compilation.Compilation, driverFactory );

            var aspectNamedTypes = GetAspectTypes( compileTimeProject );
            var aspectTypes = aspectTypeFactory.GetAspectTypes( aspectNamedTypes, diagnosticAdder ).ToImmutableArray();

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
                .GroupAdjacent( x => GetGroupingKey( x.AspectType.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), loader ) )
                .ToImmutableArray();

            configuration = new PipelineConfiguration( stages, aspectTypes, sortedAspectLayers, compileTimeProject, loader );

            return true;

            IReadOnlyList<INamedTypeSymbol> GetAspectTypes( CompileTimeProject? compileTimeProject )
                => compileTimeProject?.SelectManyRecursive( p => p.References, includeThis: true, throwOnDuplicate: false )
                       .SelectMany( p => p.AspectTypes )
                       .Select( t => compilation.Compilation.GetTypeByMetadataName( t ) )
                       .WhereNotNull()
                       .ToImmutableArray()
                   ?? ImmutableArray<INamedTypeSymbol>.Empty;

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
        /// <param name="pipelineStageResult"></param>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        private protected bool TryExecuteCore(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            PipelineConfiguration pipelineConfiguration,
            [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            // If there is no aspect in the compilation, don't execute the pipeline.
            if ( pipelineConfiguration.CompileTimeProject == null || pipelineConfiguration.AspectTypes.Count == 0 )
            {
                pipelineStageResult = new PipelineStageResult( compilation, Array.Empty<OrderedAspectLayer>() );

                return true;
            }

            var aspectSource = new CompilationAspectSource(
                pipelineConfiguration.AspectTypes,
                pipelineConfiguration.CompileTimeAssemblyLoader );

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
                diagnosticAdder.ReportDiagnostic( diagnostic );
            }

            return !hasError;
        }

        /// <summary>
        /// Creates an instance of <see cref="HighLevelPipelineStage"/>.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="compileTimeAssemblyLoader"></param>
        /// <returns></returns>
        private protected abstract HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeAssemblyLoader compileTimeAssemblyLoader );

        private PipelineStage CreateStage(
            object groupKey,
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelPipelineStage( weaver, partData.AspectType, this );

                case nameof(AspectDriver):

                    return this.CreateStage( parts, compileTimeAssemblyLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public void Dispose() { }

        public abstract bool CanTransformCompilation { get; }
    }
}