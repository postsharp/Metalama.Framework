// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly AspectType _aspectType;

        public LowLevelPipelineStage( IAspectWeaver aspectWeaver, AspectType aspectType, IAspectPipelineProperties properties ) : base( properties )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectType = aspectType;
        }

        /// <inheritdoc/>
        public override PipelineStageResult Execute( PipelineStageResult input )
        {
            // TODO: it is suboptimal to get a CompilationModel here.
            var compilationModel = CompilationModel.CreateInitialInstance( input.PartialCompilation );

            var diagnostics = new DiagnosticSink();

            var aspectInstances = input.AspectSources
                .SelectMany( s => s.GetAspectInstances( compilationModel, this._aspectType, diagnostics ) )
                .ToImmutableArray<IAspectInstance>();

            if ( !aspectInstances.Any() )
            {
                return input;
            }

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext( this._aspectType, aspectInstances, input.PartialCompilation, diagnostics.ReportDiagnostic, resources.Add );

            PartialCompilation newCompilation;

            try
            {
                newCompilation = this._aspectWeaver.Transform( context );
            }
            catch ( Exception ex )
            {
                newCompilation = context.Compilation;

                diagnostics.ReportDiagnostic(
                    GeneralDiagnosticDescriptors.ExceptionInWeaver.CreateDiagnostic( null, (this._aspectType.TypeSymbol, ex.ToDiagnosticString()) ) );
            }

            // TODO: update AspectCompilation.Aspects
            return new PipelineStageResult(
                newCompilation,
                input.AspectLayers,
                input.Diagnostics.Concat( diagnostics.ToImmutable() ),
                input.Resources.Concat( resources ).ToList(),
                input.AspectSources );
        }
    }
}