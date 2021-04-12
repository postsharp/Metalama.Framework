// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly ISdkNamedType _aspectType;

        public LowLevelPipelineStage( IAspectWeaver aspectWeaver, ISdkNamedType aspectType, IAspectPipelineProperties properties ) : base( properties )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectType = aspectType;
        }

        /// <inheritdoc/>
        public override PipelineStageResult Execute( PipelineStageResult input )
        {
            var aspectInstances = input.AspectSources.SelectMany( s => s.GetAspectInstances( this._aspectType ) ).ToImmutableArray();
            var diagnostics = new DiagnosticListBuilder();

            if ( !aspectInstances.Any() )
            {
                return input;
            }

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext( this._aspectType, aspectInstances, input.Compilation, diagnostics.ReportDiagnostic, resources.Add );

            CSharpCompilation newCompilation;
            try
            {
                newCompilation = this._aspectWeaver.Transform( context );
            }
            catch ( Exception ex )
            {
                newCompilation = context.Compilation;
                diagnostics.ReportDiagnostic( GeneralDiagnosticDescriptors.ExceptionInWeaver.CreateDiagnostic( null, (this._aspectType, ex.ToDiagnosticString()) ) );
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
