using System.Collections.Generic;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    abstract class AspectSource
    {
        public abstract IReadOnlyList<AspectInstance> GetAspects();
    }
}
