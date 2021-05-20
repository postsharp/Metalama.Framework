// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class ExpandAspectAspectPipeline : AspectPipeline
    {
        private readonly InteractiveAspectSource _source;

        private ExpandAspectAspectPipeline(
            IProjectOptions projectOptions,
            CompileTimeDomain domain,
            AspectClassMetadata aspectClass,
            ISymbol targetSymbol ) : base( projectOptions, domain )
        {
            this._source = new InteractiveAspectSource( aspectClass, targetSymbol );
        }

        private protected override IReadOnlyList<IAspectSource> CreateAspectSources( AspectPipelineConfiguration configuration ) => new[] { this._source };

        public static bool TryExecute(
            IProjectOptions projectOptions,
            CompileTimeDomain domain,
            AspectPipelineConfiguration configuration,
            AspectClassMetadata aspectClass,
            PartialCompilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            ExpandAspectAspectPipeline pipeline = new( projectOptions, domain, aspectClass, targetSymbol );

            return pipeline.TryExecute( configuration, inputCompilation, cancellationToken, out outputCompilation );
        }

        private bool TryExecute(
            AspectPipelineConfiguration designTimePipelineConfiguration,
            PartialCompilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            var pipelineConfiguration = designTimePipelineConfiguration.WithStages( s => MapPipelineStage( designTimePipelineConfiguration, s ) );

            if ( !this.TryExecuteCore( compilation, NullDiagnosticAdder.Instance, pipelineConfiguration, cancellationToken, out var result ) )
            {
                outputCompilation = null;

                return false;
            }

            outputCompilation = result.PartialCompilation.Compilation;

            return true;
        }

        private static PipelineStage MapPipelineStage( AspectPipelineConfiguration configuration, PipelineStage stage )
            => stage switch
            {
                DesignTimePipelineStage => new CompileTimePipelineStage(
                    configuration.CompileTimeProject!,
                    configuration.Layers,
                    stage.PipelineProperties ),
                _ => stage
            };

        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProject compileTimeProject,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new CompileTimePipelineStage( compileTimeProject, parts, this );

        public override bool CanTransformCompilation => true;

        private class InteractiveAspectSource : IAspectSource
        {
            private readonly AspectClassMetadata _aspectClass;
            private readonly ISymbol _targetSymbol;

            public InteractiveAspectSource( AspectClassMetadata aspectClass, ISymbol targetSymbol )
            {
                this._aspectClass = aspectClass;
                this._targetSymbol = targetSymbol;
            }

            public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

            public IEnumerable<AspectClassMetadata> AspectTypes => new[] { this._aspectClass };

            public IEnumerable<ICodeElement> GetExclusions( INamedType aspectType ) => Enumerable.Empty<ICodeElement>();

            public IEnumerable<AspectInstance> GetAspectInstances(
                CompilationModel compilation,
                AspectClassMetadata aspectClassMetadata,
                IDiagnosticAdder diagnosticAdder,
                CancellationToken cancellationToken )
            {
                var targetDeclaration = compilation.Factory.GetCodeElement( this._targetSymbol );
                var aspectInstance = (IAspect) Activator.CreateInstance( aspectClassMetadata.AspectType ).AssertNotNull();

                return new[] { aspectClassMetadata.CreateAspectInstance( aspectInstance, targetDeclaration ) };
            }
        }
    }
}