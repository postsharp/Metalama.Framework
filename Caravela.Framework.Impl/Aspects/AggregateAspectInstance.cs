using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Aspects
{
    internal interface IAspectInstanceInternal : IAspectInstance
    {
        void Skip();

         ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }
    }
    internal sealed class AggregateAspectInstance : IAspectInstanceInternal
    {
        private readonly AspectInstance _primaryInstance;

        private AggregateAspectInstance( List<AspectInstance> aspectInstances )
        {
            this._primaryInstance = aspectInstances.First();
            this.OtherInstances = aspectInstances.Skip( 1 ).Select( a => a.Aspect ).ToImmutableArray();
        }

        public static IAspectInstanceInternal GetInstance( IEnumerable<AspectInstance> aspectInstances )
        {
            var instancesList = aspectInstances.ToList();

            if ( instancesList.Count == 1 )
            {
                return instancesList[0];
            }
            else
            {
                instancesList.Sort();

                return new AggregateAspectInstance( instancesList );
            }
        }

        public IAspect Aspect => this._primaryInstance.Aspect;

        public IDeclaration TargetDeclaration => this._primaryInstance.TargetDeclaration;

        public IAspectClass AspectClass => this._primaryInstance.AspectClass;

        public bool IsSkipped => this._primaryInstance.IsSkipped;

        public ImmutableArray<IAspect> OtherInstances { get; }

        public void Skip() => this._primaryInstance.Skip();

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances => this._primaryInstance.TemplateInstances;
    }
}