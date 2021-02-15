using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using MoreLinq;

namespace Caravela.Framework.Impl.Pipeline
{

    /// <summary>
    /// The base class for the main process of Caravela.
    /// </summary>
    internal abstract class AspectPipeline : IDisposable
    {
        /// <summary>
        /// Gets the list of <see cref="AspectPart"/> in the pipeline. This list is fixed for the whole pipeline.
        /// It is based on the aspects found in the project and its dependencies.
        /// </summary>
        public IReadOnlyList<AspectPart> AspectParts { get; }

        /// <summary>
        /// Gets the list of stages of the pipeline. A stage is a group of transformations that do not require (within the group)
        /// to modify the Roslyn compilation.
        /// </summary>
        public IReadOnlyList<PipelineStage> Stages { get; }

        /// <summary>
        /// Gets the context object passed by the caller when instantiating the pipeline.
        /// </summary>
        public IAspectPipelineContext Context { get; }

        /// <summary>
        /// Gets the pipeline options.
        /// </summary>
        public IAspectPipelineOptions PipelineOptions { get; }

        // TODO: move to service provider?
        protected CompileTimeAssemblyBuilder CompileTimeAssemblyBuilder { get; }

        // TODO: move to service provider?
        protected CompileTimeAssemblyLoader CompileTimeAssemblyLoader { get; }

        protected AspectPipeline( IAspectPipelineContext context, IAspectPipelineOptions options )
        {
            if ( context.Options.GetBooleanOption( "DebugCaravela" ) )
            {
                Debugger.Launch();
            }

            this.Context = context;
            this.PipelineOptions = options;
            var roslynCompilation = context.Compilation;

            var debugTransformedCode = context.Options.GetBooleanOption( "CaravelaDebugTransformedCode" );

            // DI
            this.CompileTimeAssemblyBuilder = new CompileTimeAssemblyBuilder( roslynCompilation, context.ManifestResources, debugTransformedCode );
            this.CompileTimeAssemblyLoader = new CompileTimeAssemblyLoader( roslynCompilation, this.CompileTimeAssemblyBuilder );
            this.CompileTimeAssemblyBuilder.CompileTimeAssemblyLoader = this.CompileTimeAssemblyLoader;
            var compilation = new CompilationModel( roslynCompilation );
            var driverFactory = new AspectDriverFactory( compilation, context.Plugins );
            var aspectTypeFactory = new AspectTypeFactory( driverFactory );
            var aspectPartComparer = new AspectPartComparer();

            this.AspectParts = GetAspectTypes( compilation )
                .Select( at => aspectTypeFactory.GetAspectType( at ) )
                .SelectMany( at => at.Parts )
                .OrderBy( x => x, aspectPartComparer )
                .ToImmutableArray();

            this.Stages = this.AspectParts
                .GroupAdjacent( x => GetGroupingKey( x.AspectType.AspectDriver ) )
                .Select( g => this.CreateStage( g.Key, g.ToImmutableArray(), compilation, this.CompileTimeAssemblyLoader ) )
                .ToImmutableArray();
        }

        /// <summary>
        /// Handles an exception thrown in the pipeline.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="context"></param>
        protected static void HandleException( Exception exception, IAspectPipelineContext context )
        {
            switch ( exception )
            {
                case DiagnosticsException diagnosticsException:
                    foreach ( var diagnostic in diagnosticsException.Diagnostics )
                    {
                        context.ReportDiagnostic( diagnostic );
                    }

                    break;

                case CaravelaException caravelaException:
                    context.ReportDiagnostic( caravelaException.Diagnostic );
                    break;

                default:
                    var guid = Guid.NewGuid();
                    var path = Path.Combine( Path.GetTempPath(), $"caravela-{exception.GetType().Name}-{guid}.txt" );
                    try
                    {
                        File.WriteAllText( path, exception.ToString() );
                    }
                    catch
                    {
                    }

                    Console.WriteLine( exception.ToString() );

                    context.ReportDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.UncaughtException, null, exception.ToDiagnosticString(), path ) );
                    break;
            }
        }

        /// <summary>
        /// Executes the all stages of the current pipeline, report diagnostics, and returns the last <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="pipelineStageResult"></param>
        /// <returns><c>true</c> if there was no error, <c>false</c> otherwise.</returns>
        protected bool TryExecute( out PipelineStageResult pipelineStageResult )
        {
            pipelineStageResult = new PipelineStageResult( this.Context.Compilation, this.AspectParts );

            foreach ( var stage in this.Stages )
            {
                pipelineStageResult = stage.ToResult( pipelineStageResult );
            }

            var hasError = false;
            foreach ( var diagnostic in pipelineStageResult.Diagnostics )
            {
                this.Context.ReportDiagnostic( diagnostic );
                hasError |= diagnostic.Severity >= DiagnosticSeverity.Error;
            }

            return !hasError;
        }

        private static IEnumerable<INamedType> GetAspectTypes( CompilationModel compilation )
        {
            var iAspect = compilation.Factory.GetTypeByReflectionType( typeof( IAspect ) )!;

            return compilation.DeclaredAndReferencedTypes.Where( t => t.Is( iAspect ) );
        }

        private static object GetGroupingKey( IAspectDriver driver ) =>
            driver switch
            {
                // weavers are not grouped together
                // Note: this requires that every AspectType has its own instance of IAspectWeaver
                IAspectWeaver weaver => weaver,

                // AspectDrivers are grouped together
                AspectDriver => nameof( AspectDriver ),

                _ => throw new NotSupportedException()
            };

        /// <summary>
        /// Creates an instance of <see cref="HighLevelAspectsPipelineStage"/>.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="compileTimeAssemblyLoader"></param>
        /// <returns></returns>
        protected abstract HighLevelAspectsPipelineStage CreateStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader );

        private PipelineStage CreateStage( object groupKey, IReadOnlyList<AspectPart> parts, CompilationModel compilation, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new LowLevelAspectsPipelineStage( weaver, compilation.Factory.GetTypeByReflectionName( partData.AspectType.Name )! );

                case nameof( AspectDriver ):

                    return this.CreateStage( parts, compileTimeAssemblyLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.CompileTimeAssemblyLoader.Dispose();
        }
    }
}
