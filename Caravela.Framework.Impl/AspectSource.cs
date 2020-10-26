using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    abstract class AspectSource
    {
        public abstract IReactiveCollection<AspectInstance> GetAspects();
    }
}
