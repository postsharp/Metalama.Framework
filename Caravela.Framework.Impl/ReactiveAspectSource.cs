using Caravela.Framework.Sdk;

using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    internal class ReactiveAspectSource : AspectSource
    {
        private readonly IReadOnlyList<AspectInstance> _aspectInstances;

        public ReactiveAspectSource( IReadOnlyList<AspectInstance> aspectInstances )
        {
            this._aspectInstances = aspectInstances;
        }

        public override IReadOnlyList<AspectInstance> GetAspects() => this._aspectInstances;
    }
}
