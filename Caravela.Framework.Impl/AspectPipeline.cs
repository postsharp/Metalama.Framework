﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using MoreLinq;

namespace Caravela.Framework.Impl
{
    internal abstract class AspectPipeline
    {
        public IList<PipelineStage> Stages { get; } = new List<PipelineStage>();

        public virtual Compilation Execute( IAspectPipelineContext context )
        {
            if ( context.GetOptionsFlag( "DebugCaravela" ) )
            {
                Debugger.Launch();
            }

            try
            {
                var roslynCompilation = (CSharpCompilation) context.Compilation;

                var debugTransformedCode = context.GetOptionsFlag( "CaravelaDebugTransformedCode" );

                // DI
                var compileTimeAssemblyBuilder = new CompileTimeAssemblyBuilder( roslynCompilation, context.ManifestResources, debugTransformedCode );
                using var compileTimeAssemblyLoader = new CompileTimeAssemblyLoader( roslynCompilation, compileTimeAssemblyBuilder );
                compileTimeAssemblyBuilder.CompileTimeAssemblyLoader = compileTimeAssemblyLoader;
                var compilation = new RoslynBasedCompilationModel( roslynCompilation );
                var driverFactory = new AspectDriverFactory( compilation, context.Plugins );
                var aspectTypeFactory = new AspectTypeFactory( driverFactory );
                var aspectPartDataComparer = new AspectPartDataComparer( new AspectPartComparer() );

                var pipelineStageResult = new PipelineStageResult( roslynCompilation, Array.Empty<Diagnostic>(), Array.Empty<ResourceDescription>(), Array.Empty<AspectInstance>() );

                var stages = GetAspectTypes( compilation )
                    .Select( at => aspectTypeFactory.GetAspectType( at ) )
                    .SelectMany(
                        at => at.Parts,
                        ( aspectType, aspectPart ) => new AspectPartData( aspectType, aspectPart ) )
                    .OrderBy( x => x, aspectPartDataComparer )
                    .GroupAdjacent( x => GetGroupingKey( x.AspectType.AspectDriver ) )
                    .Select( g => CreateStage( g.Key, g, compilation, compileTimeAssemblyLoader ) );

                foreach ( var stage in stages )
                {
                    pipelineStageResult = stage.ToResult( pipelineStageResult );
                }

                foreach ( var diagnostic in pipelineStageResult.Diagnostics )
                {
                    context.ReportDiagnostic( diagnostic );
                }

                foreach ( var resource in pipelineStageResult.Resources )
                {
                    context.ManifestResources.Add( resource );
                }

                if ( roslynCompilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    var compileTimeAssembly = compileTimeAssemblyBuilder.EmitCompileTimeAssembly( roslynCompilation );

                    if ( compileTimeAssembly != null )
                    {
                        context.ManifestResources.Add( new ResourceDescription(
                            compileTimeAssemblyBuilder.GetResourceName(), () => compileTimeAssembly, isPublic: true ) );
                    }
                }

                var resultCompilation = pipelineStageResult.Compilation;

                resultCompilation = compileTimeAssemblyBuilder.PrepareRunTimeAssembly( resultCompilation );

                return resultCompilation;
            }
            catch ( CaravelaException exception )
            {
                context.ReportDiagnostic( exception.Diagnostic );

                if ( exception is DiagnosticsException diagnosticsException )
                {
                    foreach ( var diagnostic in diagnosticsException.Diagnostics )
                    {
                        context.ReportDiagnostic( diagnostic );
                    }
                }

                return context.Compilation;
            }
            catch ( Exception exception )
            {
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
                return context.Compilation;
            }
        }

        private static IEnumerable<INamedType> GetAspectTypes( RoslynBasedCompilationModel compilation )
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

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private record AspectPartData( AspectType AspectType, AspectPart AspectPart );
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

        private static PipelineStage CreateStage( object groupKey, IEnumerable<AspectPartData> partsData, CompilationModel compilation, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = partsData.Single();

                    return new AspectWeaverStage( weaver, compilation.GetTypeByReflectionName( partData.AspectType.Name )! );

                case nameof( AspectDriver ):

                    return new AdviceWeaverStage( partsData.Select( pd => pd.AspectPart ), compileTimeAssemblyLoader );

                default:

                    throw new NotSupportedException();
            }
        }

        private class AspectPartDataComparer : IComparer<AspectPartData>
        {
            private readonly AspectPartComparer _partComparer;

            public AspectPartDataComparer( AspectPartComparer partComparer )
            {
                this._partComparer = partComparer;
            }

            public int Compare( AspectPartData x, AspectPartData y ) => this._partComparer.Compare( x.AspectPart, y.AspectPart );
        }
    }
}
