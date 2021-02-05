using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal sealed class AspectPipeline
    {
        public Compilation Execute( IAspectPipelineContext context )
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
                var compilation = new SourceCompilation( roslynCompilation );
                var driverFactory = new AspectDriverFactory( compilation, context.Plugins );
                var aspectTypeFactory = new AspectTypeFactory( driverFactory );
                var aspectPartDataComparer = new AspectPartDataComparer( new AspectPartComparer() );

                var aspectCompilation = new AspectCompilation( compilation, compileTimeAssemblyLoader );

                var stages = GetAspectTypes( compilation )
                    .Select( at => aspectTypeFactory.GetAspectType( at ) )
                    .SelectMany(
                        at => at.Parts,
                        ( aspectType, aspectPart ) => new AspectPartData( aspectType, aspectPart ) )
                    .OrderedGroupBy( aspectPartDataComparer, x => GetGroupingKey( x.AspectType.AspectDriver ) )
                    .Select( g => CreateStage( g.Key, g.GetValue(), compilation ) )
                    .GetValue();

                foreach ( var stage in stages )
                {
                    aspectCompilation = stage.Transform( aspectCompilation );
                }

                foreach ( var diagnostic in aspectCompilation.Diagnostics )
                {
                    context.ReportDiagnostic( diagnostic );
                }

                foreach ( var resource in aspectCompilation.Resources )
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

                var resultCompilation = aspectCompilation.Compilation.GetRoslynCompilation();

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
                    // ignored
                }

                Console.WriteLine( exception.ToString() );

                context.ReportDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.UncaughtException, null, exception.ToDiagnosticString(), path ) );
                return context.Compilation;
            }
        }

        private static IReactiveCollection<INamedType> GetAspectTypes( SourceCompilation compilation )
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

        private static PipelineStage CreateStage( object groupKey, IEnumerable<AspectPartData> partsData, ICompilation compilation )
        {
            switch ( groupKey )
            {
                case IAspectWeaver weaver:

                    var partData = partsData.Single();

                    return new AspectWeaverStage( weaver, compilation.GetTypeByReflectionName( partData.AspectType.Name )! );

                case nameof( AspectDriver ):

                    return new AdviceWeaverStage( partsData.Select( pd => pd.AspectPart ) );

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

        private record AspectPartData( AspectType AspectType, AspectPart AspectPart );
    }
}
