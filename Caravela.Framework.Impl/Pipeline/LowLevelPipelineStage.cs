// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Sdk;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly AspectClass _aspectClass;

        public LowLevelPipelineStage( IAspectWeaver aspectWeaver, AspectClass aspectClass, IServiceProvider serviceProvider ) : base( serviceProvider )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectClass = aspectClass;
        }

        /// <inheritdoc/>
        public override bool TryExecute(
            PipelineStageResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? result )
        {
            // TODO: it is suboptimal to get a CompilationModel here.
            var compilationModel = CompilationModel.CreateInitialInstance( input.PartialCompilation );

            var aspectInstances = input.AspectSources
                .SelectMany( s => s.GetAspectInstances( compilationModel, this._aspectClass, diagnostics, cancellationToken ) )
                .ToImmutableDictionary(
                    i => i.TargetDeclaration.GetSymbol().AssertNotNull( "The Roslyn compilation should include all introduced declarations." ),
                    i => (IAspectInstance) i );

            if ( !aspectInstances.Any() )
            {
                result = input;

                return true;
            }

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext(
                this._aspectClass,
                aspectInstances,
                input.PartialCompilation,
                diagnostics.Report,
                resources.Add );

            PartialCompilation newCompilation;

            try
            {
                this._aspectWeaver.Transform( context );
                newCompilation = (PartialCompilation) context.Compilation;
            }
            catch ( Exception ex )
            {
                diagnostics.Report(
                    GeneralDiagnosticDescriptors.ExceptionInWeaver.CreateDiagnostic( null, (this._aspectClass.DisplayName, ex.ToDiagnosticString()) ) );

                result = null;

                return false;
            }

            // TODO: update AspectCompilation.Aspects
            result = new PipelineStageResult(
                newCompilation,
                input.AspectLayers,
                input.Diagnostics,
                input.AspectSources );

            return true;
        }
    }
}