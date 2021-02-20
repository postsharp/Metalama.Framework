using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelAspectsPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly INamedType _aspectType;

        public LowLevelAspectsPipelineStage( IAspectWeaver aspectWeaver, INamedType aspectType, IAspectPipelineProperties properties ) : base( properties )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectType = aspectType;
        }

        /// <inheritdoc/>
        public override PipelineStageResult Execute( PipelineStageResult input )
        {
            var aspectInstances = input.AspectSources.SelectMany( s => s.GetAspectInstances( this._aspectType ) ).ToImmutableArray();
            var diagnostics = new List<Diagnostic>();

            if ( !aspectInstances.Any() )
            {
                return input;
            }


            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext( this._aspectType, aspectInstances, input.Compilation, diagnostics.Add, resources.Add );

            CSharpCompilation newCompilation;
            try
            {
                newCompilation = this._aspectWeaver.Transform( context );
            }
            catch ( Exception ex )
            {
                newCompilation = context.Compilation;
                diagnostics.Add( Diagnostic.Create( GeneralDiagnosticDescriptors.ExceptionInWeaver, null, this._aspectType, ex.ToDiagnosticString() ) );
            }

            // TODO: update AspectCompilation.Aspects
            return new PipelineStageResult(
                newCompilation,
                input.AspectLayers,
                input.Diagnostics.Concat( diagnostics ).ToList(),
                input.Resources.Concat( resources ).ToList(),
                input.AspectSources );
        }
    }
  
}
