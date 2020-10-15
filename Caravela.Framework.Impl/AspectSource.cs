using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    abstract class AspectSource
    {
        public abstract IReadOnlyList<AspectInstance> GetAspects();
    }
}
