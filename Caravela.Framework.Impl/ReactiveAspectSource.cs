using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    internal class ReactiveAspectSource : AspectSource
    {
        private readonly IReactiveCollection<AspectInstance> _aspectInstances;

        public ReactiveAspectSource( IReactiveCollection<AspectInstance> aspectInstances ) => this._aspectInstances = aspectInstances;

        public override IReactiveCollection<AspectInstance> GetAspects() => this._aspectInstances;
    }
}
