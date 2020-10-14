using System.Collections.Generic;
using PostSharp.Framework.Sdk;

namespace PostSharp.Framework.Impl
{
    abstract class AspectSource
    {
        public abstract IReadOnlyList<AspectInstance> GetAspects();
    }
}
