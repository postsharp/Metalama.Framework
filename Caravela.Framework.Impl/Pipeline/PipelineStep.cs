using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class PipelineStep
    {
        private List<Advice> _advices = new List<Advice>();
         
        public PipelineStepId Id { get; }
        public OrderedAspectLayer AspectLayer { get; }

        public PipelineStep( PipelineStepId id, OrderedAspectLayer aspectLayer )
        {
            this.Id = id;
            this.AspectLayer = aspectLayer;
        }

        public virtual CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState )
        {
            var adviceResults = this._advices
                .Select( ai => ai.ToResult( compilation ) ).ToList();

            var addedObservableIntroductions = adviceResults.SelectMany( ar => ar.ObservableTransformations );
            var addedNonObservableTransformations = adviceResults.SelectMany( ar => ar.NonObservableTransformations );
            
            pipelineStepsState.AddNonObservableTransformations( addedNonObservableTransformations );

            return CompilationModel.CreateRevisedInstance(compilation, addedObservableIntroductions);
        }

        public void AddAdvice( Advice advice ) => this._advices.Add( advice );

        
    }
}