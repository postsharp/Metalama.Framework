// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline.LiveTemplates
{
    /// <summary>
    /// An implementation of the <see cref="AspectPipeline"/> that applies an aspect to source code in the interactive process.
    /// </summary>
    internal class LiveTemplateAspectPipeline : AspectPipeline
    {
        private readonly Func<AspectPipelineConfiguration, IAspectClass> _aspectSelector;
        private readonly ISymbol _targetSymbol;

        private LiveTemplateAspectPipeline(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            Func<AspectPipelineConfiguration, IAspectClass> aspectSelector,
            ISymbol targetSymbol ) : base( serviceProvider, ExecutionScenario.LiveTemplate, false, domain )
        {
            this._aspectSelector = aspectSelector;
            this._targetSymbol = targetSymbol;
        }

        private protected override (ImmutableArray<IAspectSource> AspectSources, ImmutableArray<IValidatorSource> ValidatorSources) CreateAspectSources(
            AspectPipelineConfiguration configuration,
            Compilation compilation,
            CancellationToken cancellationToken )
        {
            var aspectClass = this._aspectSelector( configuration );

            return (ImmutableArray.Create<IAspectSource>( new AspectSource( this, aspectClass ) ), ImmutableArray<IValidatorSource>.Empty);
        }

        public static bool TryExecute(
            ServiceProvider serviceProvider,
            CompileTimeDomain domain,
            AspectPipelineConfiguration? pipelineConfiguration,
            Func<AspectPipelineConfiguration, IAspectClass> aspectSelector,
            PartialCompilation inputCompilation,
            ISymbol targetSymbol,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PartialCompilation? outputCompilation )
        {
            LiveTemplateAspectPipeline pipeline = new( serviceProvider, domain, aspectSelector, targetSymbol );

            if ( !pipeline.TryExecute( inputCompilation, diagnosticAdder, pipelineConfiguration, cancellationToken, out var result ) )
            {
                outputCompilation = null;

                return false;
            }

            outputCompilation = result.Compilation;

            return true;
        }

        private protected override HighLevelPipelineStage CreateHighLevelStage(
            PipelineStageConfiguration configuration,
            CompileTimeProject compileTimeProject )
            => new CompileTimePipelineStage( compileTimeProject, configuration.Parts, this.ServiceProvider );

        private class AspectSource : IAspectSource
        {
            private readonly LiveTemplateAspectPipeline _parent;

            public AspectSource( LiveTemplateAspectPipeline parent, IAspectClass aspectClass )
            {
                this._parent = parent;

                this.AspectClasses = ImmutableArray.Create( aspectClass );
            }

            public ImmutableArray<IAspectClass> AspectClasses { get; }

            public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => Enumerable.Empty<IDeclaration>();

            public IEnumerable<AspectInstance> GetAspectInstances(
                CompilationModel compilation,
                IAspectClass aspectClass,
                IDiagnosticAdder diagnosticAdder,
                CancellationToken cancellationToken )
            {
                var targetDeclaration = compilation.Factory.GetDeclaration( this._parent._targetSymbol );

                return new[]
                {
                    ((AspectClass) aspectClass).CreateAspectInstance(
                        targetDeclaration,
                        (IAspect) Activator.CreateInstance( this.AspectClasses[0].Type ),
                        default )
                };
            }
        }
    }
}