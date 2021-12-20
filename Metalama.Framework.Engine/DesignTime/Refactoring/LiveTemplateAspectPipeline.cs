// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.DesignTime.Refactoring
{
    /// <summary>
    /// An implementation of the <see cref="AspectPipeline"/> that applies an aspect to source code in the interactive process.
    /// </summary>
    internal class LiveTemplateAspectPipeline : AspectPipeline
    {
        private readonly InteractiveAspectSource _source;

        private LiveTemplateAspectPipeline(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AspectClass aspectClass,
            IAspect aspect,
            ISymbol targetSymbol ) : base( serviceProvider, ExecutionScenario.LiveTemplate, false, domain )
        {
            this._source = new InteractiveAspectSource( aspectClass, aspect, targetSymbol );
        }

        private protected override (ImmutableArray<IAspectSource> AspectSources, ImmutableArray<IValidatorSource> ValidatorSources) CreateAspectSources(
            AspectPipelineConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
            => (ImmutableArray.Create<IAspectSource>( this._source ), ImmutableArray<IValidatorSource>.Empty);

        public static bool TryExecute(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AspectPipelineConfiguration configuration,
            AspectClass aspectClass,
            IAspect aspect,
            PartialCompilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            LiveTemplateAspectPipeline pipeline = new( serviceProvider, domain, aspectClass, aspect, targetSymbol );

            return pipeline.TryExecute( configuration, inputCompilation, cancellationToken, out outputCompilation, out diagnostics );
        }

        private bool TryExecute(
            AspectPipelineConfiguration pipelineConfiguration,
            PartialCompilation compilation,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            DiagnosticList diagnosticList = new();

            if ( !this.TryExecute( compilation, diagnosticList, pipelineConfiguration, cancellationToken, out var result ) )
            {
                outputCompilation = null;
                diagnostics = diagnosticList.ToImmutableArray();

                return false;
            }

            outputCompilation = result.Compilation;
            diagnostics = ImmutableArray<Diagnostic>.Empty;

            return true;
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );

        private class InteractiveAspectSource : IAspectSource
        {
            private readonly ISymbol _targetSymbol;
            private readonly IAspect _aspect;

            public InteractiveAspectSource( AspectClass aspectClass, IAspect aspect, ISymbol targetSymbol )
            {
                this._targetSymbol = targetSymbol;
                this._aspect = aspect;
                this.AspectClasses = ImmutableArray.Create<IAspectClass>( aspectClass );
            }

            public ImmutableArray<IAspectClass> AspectClasses { get; }

            public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

            public IEnumerable<AspectInstance> GetAspectInstances(
                CompilationModel compilation,
                IAspectClass aspectClass,
                IDiagnosticAdder diagnosticAdder,
                CancellationToken cancellationToken )
            {
                var targetDeclaration = compilation.Factory.GetDeclaration( this._targetSymbol );

                return new[] { ((AspectClass) aspectClass).CreateAspectInstance( targetDeclaration, this._aspect, default ) };
            }
        }
    }
}