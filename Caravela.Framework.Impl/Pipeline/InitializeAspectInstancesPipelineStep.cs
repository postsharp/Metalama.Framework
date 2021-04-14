// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The <see cref="PipelineStep"/> that runs the default layer of each aspect. It runs the aspect initializer method.
    /// </summary>
    internal class InitializeAspectInstancesPipelineStep : AdvicePipelineStep
    {
        private readonly List<AspectInstance> _aspectInstances = new();

        public InitializeAspectInstancesPipelineStep( PipelineStepId stepId, OrderedAspectLayer aspectLayer ) : base( stepId, aspectLayer )
        {
        }

        public void AddAspectInstance( AspectInstance aspectInstance ) => this._aspectInstances.Add( aspectInstance );

        public override CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState )
        {
            var aspectDriver = (AspectDriver) this.AspectLayer.AspectType.AspectDriver;

            var aspectInstanceResults = this._aspectInstances.Select( ai => aspectDriver.EvaluateAspect( ai ) ).ToImmutableArray();
            var success = aspectInstanceResults.All( ar => ar.Success );
            var reportedDiagnostics = aspectInstanceResults.SelectMany( air => air.Diagnostics.ReportedDiagnostics );
            var diagnosticSuppressions = aspectInstanceResults.SelectMany( air => air.Diagnostics.DiagnosticSuppressions );
            var addedAspectSources = aspectInstanceResults.SelectMany( air => air.AspectSources );
            var addedAdvices = aspectInstanceResults.SelectMany( air => air.Advices ).Cast<Advice>();

            pipelineStepsState.AddDiagnostics( reportedDiagnostics, diagnosticSuppressions );
            success &= pipelineStepsState.AddAspectSources( addedAspectSources );
            success &= pipelineStepsState.AddAdvices( addedAdvices );

            // It's not clear if we should continue at that time. An error here may result in more errors later.
            _ = success;

            return base.Execute( compilation, pipelineStepsState );
        }
    }
}