// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Sdk;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly IBoundAspectClass _aspectClass;

        public LowLevelPipelineStage( IAspectWeaver aspectWeaver, IBoundAspectClass aspectClass, IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectClass = aspectClass;
        }

        /// <inheritdoc/>
        public override bool TryExecute(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? result )
        {
            // TODO: it is suboptimal to get a CompilationModel here.
            var compilationModel = CompilationModel.CreateInitialInstance( input.Project, input.Compilation );
            var compilation = input.Compilation.Compilation;

            var aspectInstances = input.AspectSources
                .SelectMany( s => s.GetAspectInstances( compilationModel, this._aspectClass, diagnostics, cancellationToken ) )
                .GroupBy(
                    i => i.TargetDeclaration.GetSymbol( compilation ).AssertNotNull( "The Roslyn compilation should include all introduced declarations." ) )
                .ToImmutableDictionary( g => g.Key, g => (IAspectInstance) AggregateAspectInstance.GetInstance( g ) );

            if ( !aspectInstances.Any() )
            {
                result = input;

                return true;
            }

            var resources = new List<ManagedResource>();

            var context = new AspectWeaverContext(
                this._aspectClass,
                aspectInstances,
                input.Compilation,
                diagnostics.Report,
                resources.Add,
                new AspectWeaverHelper( pipelineConfiguration.ServiceProvider, compilation ),
                pipelineConfiguration.ServiceProvider,
                input.Project );

            var executionContext = new UserCodeExecutionContext(
                this.ServiceProvider,
                diagnostics,
                UserCodeMemberInfo.FromDelegate( new Action<AspectWeaverContext>( this._aspectWeaver.Transform ) ) );

            if ( !this.ServiceProvider.GetService<UserCodeInvoker>().TryInvoke( () => this._aspectWeaver.Transform( context ), executionContext ) )
            {
                result = null;

                return false;
            }

            var newCompilation = (PartialCompilation) context.Compilation;

            // TODO: update AspectCompilation.Aspects
            result = new PipelineStageResult(
                newCompilation,
                input.Project,
                input.AspectLayers,
                null,
                input.Diagnostics,
                input.AspectSources );

            return true;
        }
    }
}