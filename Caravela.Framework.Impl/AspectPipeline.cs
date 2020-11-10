﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynEx;

namespace Caravela.Framework.Impl
{
    [Transformer]
    sealed class AspectPipeline : ISourceTransformer
    {
        public Compilation Execute(TransformerContext context)
        {
            bool getFlag( string flagName ) =>
                context.GlobalOptions.TryGetValue( $"build_property.{flagName}", out var flagString ) &&
                bool.TryParse( flagString, out bool flagValue ) &&
                flagValue;

            if ( getFlag("DebugCaravela") )
            {
                Debugger.Launch();
            }

            try
            {
                var roslynCompilation = (CSharpCompilation) context.Compilation;

                bool debugTransformedCode = getFlag( "RoslynExDebugTransformedCode" );

                // DI
                var compileTimeAssemblyBuilder = new CompileTimeAssemblyBuilder( new SymbolClassifier( roslynCompilation ), new TemplateCompiler(), debugTransformedCode );
                using var compileTimeAssemblyLoader = new CompileTimeAssemblyLoader( roslynCompilation, compileTimeAssemblyBuilder );
                compileTimeAssemblyBuilder.CompileTimeAssemblyLoader = compileTimeAssemblyLoader;
                var compilation = new SourceCompilation( roslynCompilation );
                var driverFactory = new AspectDriverFactory( compilation, compileTimeAssemblyLoader );
                var aspectTypeFactory = new AspectTypeFactory( driverFactory );
                var aspectPartDataComparer = new AspectPartDataComparer( new AspectPartComparer() );

                var aspectCompilation = new AspectCompilation( ImmutableArray.Create<Diagnostic>(), compilation, compileTimeAssemblyLoader );

                var stages = GetAspectTypes( compilation )
                    .Select( at => aspectTypeFactory.GetAspectType( at ) )
                    .SelectMany(
                        at => at.Parts,
                        ( aspectType, aspectPart ) => new AspectPartData( aspectType, aspectPart ) )
                    .OrderedGroupBy( aspectPartDataComparer, x => GetGroupingKey( x.AspectType.AspectDriver ) )
                    .Select( g => CreateStage( g.Key, g.GetValue(), compilation ) )
                    .GetValue( default );

                foreach ( var stage in stages )
                {
                    aspectCompilation = stage.Transform( aspectCompilation );
                }

                foreach ( var diagnostic in aspectCompilation.Diagnostics )
                {
                    context.ReportDiagnostic( diagnostic );
                }

                if ( roslynCompilation.Options.OutputKind == OutputKind.DynamicallyLinkedLibrary )
                {
                    var compileTimeAssembly = compileTimeAssemblyBuilder.EmitCompileTimeAssembly( roslynCompilation );

                    if ( compileTimeAssembly != null )
                    {
                        context.AddManifestResource( new ResourceDescription(
                            compileTimeAssemblyBuilder.GetResourceName(), () => compileTimeAssembly, isPublic: true ) );
                    }
                }

                return aspectCompilation.Compilation.GetRoslynCompilation();
            }
            catch (CaravelaException exception)
            {
                context.ReportDiagnostic( exception.Diagnostic );

                if (exception is DiagnosticsException diagnosticsException)
                {
                    foreach ( var diagnostic in diagnosticsException.Diagnostics )
                        context.ReportDiagnostic( diagnostic );
                }

                return context.Compilation;
            }
            catch (Exception exception)
            {
                static string ToString( Exception ex ) =>
                    ex.InnerException == null ? $"{ex.GetType()}: {ex.Message}" : $"{ex.GetType()}: {ex.Message} -> {ToString( ex.InnerException )}";

                context.ReportDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.UncaughtException, null, ToString( exception ) ) );
                return context.Compilation;
            }
        }

        private static IReactiveCollection<INamedType> GetAspectTypes(SourceCompilation compilation)
        {
            var iAspect = compilation.GetTypeByReflectionType( typeof( IAspect ) )!;

            return compilation.DeclaredAndReferencedTypes.Where( t => t.IsDefaultConstructible && t.Is( iAspect ) );
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


        record AspectPartData( AspectType AspectType, AspectPart AspectPart );

        private static PipelineStage CreateStage( object groupKey, IEnumerable<AspectPartData> partsData, ICompilation compilation )
        {
            switch (groupKey)
            {
                case IAspectWeaver weaver:

                    var partData = partsData.Single();

                    return new AspectWeaverStage( weaver, compilation.GetTypeByReflectionName( partData.AspectType.Name )! );

                case nameof( AspectDriver ):

                    return new AdviceWeaverStage(partsData.Select(pd => pd.AspectPart));

                default:

                    throw new NotSupportedException();
            };
        }

        class AspectPartDataComparer : IComparer<AspectPartData>
        {
            private readonly AspectPartComparer _partComparer;

            public AspectPartDataComparer( AspectPartComparer partComparer ) => this._partComparer = partComparer;

            public int Compare( AspectPartData x, AspectPartData y ) => this._partComparer.Compare( x.AspectPart, y.AspectPart );
        }
    }
}
