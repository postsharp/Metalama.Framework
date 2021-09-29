// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Refactoring
{
    /// <summary>
    /// An implementation of the <see cref="AspectPipeline"/> that applies an aspect to source code in the interactive process.
    /// </summary>
    internal class LiveTemplateAspectPipeline : AspectPipeline
    {
        private readonly InteractiveAspectSource _source;

        private LiveTemplateAspectPipeline(
            IProjectOptions projectOptions,
            CompileTimeDomain domain,
            AspectClass aspectClass,
            ISymbol targetSymbol ) : base( projectOptions, AspectExecutionScenario.LiveTemplate, false, domain )
        {
            this._source = new InteractiveAspectSource( aspectClass, targetSymbol );
        }

        private protected override ImmutableArray<IAspectSource> CreateAspectSources(
            AspectProjectConfiguration configuration,
            Compilation compilation )
            => ImmutableArray.Create<IAspectSource>( this._source );

        public static bool TryExecute(
            IProjectOptions projectOptions,
            CompileTimeDomain domain,
            AspectProjectConfiguration configuration,
            AspectClass aspectClass,
            PartialCompilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            LiveTemplateAspectPipeline pipeline = new( projectOptions, domain, aspectClass, targetSymbol );

            return pipeline.TryExecute( configuration, inputCompilation, cancellationToken, out outputCompilation, out diagnostics );
        }

        private bool TryExecute(
            AspectProjectConfiguration designTimeProjectConfiguration,
            PartialCompilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            var pipelineConfiguration = designTimeProjectConfiguration.WithStages( s => MapPipelineStage( designTimeProjectConfiguration, s ) );

            DiagnosticList diagnosticList = new();

            if ( !this.TryExecute( compilation, diagnosticList, pipelineConfiguration, cancellationToken, out var result ) )
            {
                outputCompilation = null;
                diagnostics = diagnosticList.ToImmutableArray();

                return false;
            }

            outputCompilation = result.PartialCompilation;
            diagnostics = ImmutableArray<Diagnostic>.Empty;

            return true;
        }

        private static PipelineStage MapPipelineStage( AspectProjectConfiguration configuration, PipelineStage stage )
            => stage switch
            {
                SourceGeneratorPipelineStage => new CompileTimePipelineStage(
                    configuration.CompileTimeProject!,
                    configuration.AspectLayers,
                    stage.ServiceProvider ),
                _ => stage
            };

        private protected override HighLevelPipelineStage CreateStage(
            ImmutableArray<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new CompileTimePipelineStage( compileTimeProject, parts, this.ServiceProvider );

        private class InteractiveAspectSource : IAspectSource
        {
            private readonly ISymbol _targetSymbol;

            public InteractiveAspectSource( AspectClass aspectClass, ISymbol targetSymbol )
            {
                this._targetSymbol = targetSymbol;
                this.AspectClasses = ImmutableArray.Create<IAspectClass>( aspectClass );
            }

            public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

            public ImmutableArray<IAspectClass> AspectClasses { get; }

            public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

            public IEnumerable<AspectInstance> GetAspectInstances(
                CompilationModel compilation,
                IAspectClass aspectClass,
                IDiagnosticAdder diagnosticAdder,
                CancellationToken cancellationToken )
            {
                var targetDeclaration = compilation.Factory.GetDeclaration( this._targetSymbol );

                return new[] { ((AspectClass) aspectClass).CreateDefaultAspectInstance( targetDeclaration ) };
            }
        }
    }
}