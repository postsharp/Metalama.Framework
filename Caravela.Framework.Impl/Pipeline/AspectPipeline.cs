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
    internal abstract class AspectPipeline : IDisposable, IAspectPipelineProperties
    {
        protected ServiceProvider ServiceProvider { get; } = new ServiceProvider();

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

        // TODO: move to service provider?
        protected CompileTimeAssemblyBuilder CompileTimeAssemblyBuilder { get; }

        // TODO: move to service provider?
        protected CompileTimeAssemblyLoader CompileTimeAssemblyLoader { get; }

        protected AspectPipeline( IAspectPipelineContext context )
        {
            if ( context.BuildOptions.AttachDebugger )
            {
                Debugger.Launch();
            }

            this.ServiceProvider.AddService( context.BuildOptions );

            this.Context = context;
            var roslynCompilation = context.Compilation;

            var debugTransformedCode = context.BuildOptions.MapPdbToTransformedCode;

            // DI
            this.CompileTimeAssemblyBuilder = new CompileTimeAssemblyBuilder( this.ServiceProvider, roslynCompilation, context.ManifestResources );
            this.CompileTimeAssemblyLoader = new CompileTimeAssemblyLoader( this.ServiceProvider, roslynCompilation, this.CompileTimeAssemblyBuilder );
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
        protected void HandleException( Exception exception )
        {
            switch ( exception )
            {
                case InvalidUserCodeException diagnosticsException:
                    foreach ( var diagnostic in diagnosticsException.Diagnostics )
                    {
                        this.Context.ReportDiagnostic( diagnostic );
                    }

                    break;

                default:
                    if ( this.WriteUnhandledExceptionsToFile )
                    {
                        var guid = Guid.NewGuid();
                        var crashReportDirectory = this.Context.BuildOptions.CrashReportDirectory;
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
                        catch
                        {
                        }

                        Console.WriteLine( exception.ToString() );

                        this.Context.ReportDiagnostic( Diagnostic.Create(
                            GeneralDiagnosticDescriptors.UncaughtException,
                            null,
                            exception.ToDiagnosticString(),
                            path ) );
                    }
                    else
                    {
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
        protected bool TryExecuteCore( out PipelineStageResult pipelineStageResult )
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

                    return new LowLevelAspectsPipelineStage( weaver, (ISdkNamedType) compilation.Factory.GetTypeByReflectionName( partData.AspectType.Name )!, this );

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

        public abstract bool CanTransformCompilation { get; }
    }
}
