// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
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
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AspectClass aspectClass,
            ISymbol targetSymbol ) : base( serviceProvider, AspectExecutionScenario.LiveTemplate, false, domain )
        {
            this._source = new InteractiveAspectSource( aspectClass, targetSymbol );
        }

        private protected override ImmutableArray<IAspectSource> CreateAspectSources(
            AspectPipelineConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
            => ImmutableArray.Create<IAspectSource>( this._source );

        public static bool TryExecute(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AspectPipelineConfiguration configuration,
            AspectClass aspectClass,
            PartialCompilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation,
            out ImmutableArray<Diagnostic> diagnostics )
        {
            LiveTemplateAspectPipeline pipeline = new( serviceProvider, domain, aspectClass, targetSymbol );

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

            outputCompilation = result.PartialCompilation;
            diagnostics = ImmutableArray<Diagnostic>.Empty;

            return true;
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            AspectPipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );

        private class InteractiveAspectSource : IAspectSource
        {
            private readonly ISymbol _targetSymbol;

            public InteractiveAspectSource( AspectClass aspectClass, ISymbol targetSymbol )
            {
                this._targetSymbol = targetSymbol;
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

                return new[] { ((AspectClass) aspectClass).CreateDefaultAspectInstance( targetDeclaration, default ) };
            }
        }
    }
}