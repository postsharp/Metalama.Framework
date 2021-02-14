using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using MoreLinq;

namespace Caravela.Framework.Impl
{

    internal abstract class AspectPipeline : IDisposable
    {
        public IReadOnlyList<AspectPart> AspectParts { get;}

        public IReadOnlyList<PipelineStage> Stages { get;}

        public IAspectPipelineContext Context { get;  }

        public IAspectPipelineOptions PipelineOptions { get; }

        protected CompileTimeAssemblyBuilder CompileTimeAssemblyBuilder { get; }

        protected CompileTimeAssemblyLoader CompileTimeAssemblyLoader { get;  }

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


        protected bool TryExecute(  out PipelineStageResult pipelineStageResult )
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
            var iAspect = compilation.GetTypeByReflectionType( typeof( IAspect ) )!;

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

        protected abstract AdviceWeaverStage CreateAdviceWeaverStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader );

        private PipelineStage CreateStage( object groupKey, IReadOnlyList<AspectPart> parts, CompilationModel compilation, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = parts.Single();

                    return new AspectWeaverStage( weaver, compilation.GetTypeByReflectionName( partData.AspectType.Name )! );

                case nameof( AspectDriver ):

                    return this.CreateAdviceWeaverStage( parts, compileTimeAssemblyLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        public void Dispose()
        {
            this.CompileTimeAssemblyLoader.Dispose();
        }
    }
}
