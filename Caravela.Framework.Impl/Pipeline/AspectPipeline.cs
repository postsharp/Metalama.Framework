﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The base class for the main process of Caravela.
    /// </summary>
    public abstract class AspectPipeline : IDisposable, IAspectPipelineProperties
    {
        protected ServiceProvider ServiceProvider { get; } = new();

        private IReadOnlyList<OrderedAspectLayer>? _aspectLayers;

        /// <summary>
        /// Gets the list of stages of the pipeline. A stage is a group of transformations that do not require (within the group)
        /// to modify the Roslyn compilation.
        /// </summary>
        private IReadOnlyList<PipelineStage>? _stages;

        /// <summary>
        /// Gets the context object passed by the caller when instantiating the pipeline.
        /// </summary>
        protected IAspectPipelineContext Context { get; }

        // TODO: move to service provider?
        private protected CompileTimeAssemblyLoader? CompileTimeAssemblyLoader { get; private set; }

        protected AspectPipeline( IAspectPipelineContext context )
        {
            if ( context.BuildOptions.AttachDebugger )
            {
                Debugger.Launch();
            }

            this.ServiceProvider.AddService( context.BuildOptions );

            this.Context = context;
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
                        var crashReportDirectory = this.Context.BuildOptions.GetCrashReportDirectoryOrDefault();

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

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="pipelineStageResult"></param>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        private protected bool TryExecuteCore( IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out PipelineStageResult? pipelineStageResult )
        {
            var roslynCompilation = this.Context.Compilation;
            var compilation = CompilationModel.CreateInitialInstance( roslynCompilation );

            // Create dependencies.
            this.CompileTimeAssemblyLoader = CompileTimeAssemblyLoader.Create( this.ServiceProvider, roslynCompilation );

            // Prepare the compile-time assembly.
            if ( !this.CompileTimeAssemblyLoader.TryLoadCompileTimeAssembly( compilation.RoslynCompilation.Assembly, diagnosticAdder, out _ ) )
            {
                pipelineStageResult = null;

                return false;
            }

            // Create aspect types.
            var driverFactory = new AspectDriverFactory( compilation, this.Context.Plugins );
            var aspectTypeFactory = new AspectTypeFactory( compilation, driverFactory );

            var aspectTypes = aspectTypeFactory.GetAspectTypes( GetAspectTypes( compilation ), diagnosticAdder ).ToImmutableArray();

            // Get aspect parts and sort them.
            var unsortedAspectLayers = aspectTypes
                .Where( t => !t.IsAbstract )
                .SelectMany( at => at.Layers )
                .ToImmutableArray();

            var aspectOrderSources = new IAspectOrderingSource[]
            {
                new AttributeAspectOrderingSource( compilation ), new AspectLayerOrderingSource( aspectTypes )
            };

            if ( !AspectLayerSorter.TrySort( unsortedAspectLayers, aspectOrderSources, diagnosticAdder, out var sortedAspectLayers ) )
            {
                pipelineStageResult = null;

                return false;
            }

            this._aspectLayers = sortedAspectLayers.ToImmutableArray();

            this._stages = this._aspectLayers
                .GroupAdjacent( x => GetGroupingKey( x.AspectType.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), this.CompileTimeAssemblyLoader ) )
                .ToImmutableArray();

            pipelineStageResult = new PipelineStageResult( this.Context.Compilation, this._aspectLayers );

            foreach ( var stage in this._stages )
            {
                pipelineStageResult = stage.Execute( pipelineStageResult );
            }

            var hasError = pipelineStageResult.Diagnostics.ReportedDiagnostics.Any( d => d.Severity >= DiagnosticSeverity.Error );

            foreach ( var diagnostic in pipelineStageResult.Diagnostics.ReportedDiagnostics )
            {
                diagnosticAdder.ReportDiagnostic( diagnostic );
            }

            return !hasError;
        }

        private static IReadOnlyList<INamedType> GetAspectTypes( CompilationModel compilation )
        {
            var iAspect = compilation.Factory.GetTypeByReflectionType( typeof(IAspect) )!;

            // We need to return abstract classes but not interfaces.
            return compilation.DeclaredAndReferencedTypes.Where( t => t.Is( iAspect ) && t.TypeKind == TypeKind.Class ).ToReadOnlyList();
        }

        private static object GetGroupingKey( IAspectDriver driver )
            => driver switch
            {
                // weavers are not grouped together
                // Note: this requires that every AspectType has its own instance of IAspectWeaver
                IAspectWeaver weaver => weaver,

                // AspectDrivers are grouped together
                AspectDriver => nameof(AspectDriver),

                _ => throw new NotSupportedException()
            };

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
        public void Dispose()
        {
            this.CompileTimeAssemblyLoader?.Dispose();
        }

        public abstract bool CanTransformCompilation { get; }
    }
}