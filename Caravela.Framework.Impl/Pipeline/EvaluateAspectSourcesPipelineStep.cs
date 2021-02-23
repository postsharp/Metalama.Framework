using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class EvaluateAspectSourcesPipelineStep : PipelineStep
    {
        private List<IAspectSource> _aspectSources = new List<IAspectSource>();

        public EvaluateAspectSourcesPipelineStep( OrderedAspectLayer aspectLayer ) : base( new PipelineStepId( aspectLayer.AspectLayerId, -1 ), aspectLayer )
        {
        }

        public override CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState )
        {
            pipelineStepsState.AddAspectInstances( this._aspectSources.SelectMany( s => s.GetAspectInstances( this.AspectLayer.AspectType.Type ) ) );

            return base.Execute( compilation, pipelineStepsState );
        }

        public void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );
    }
}