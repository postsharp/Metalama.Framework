using System.Collections.Generic;

namespace Caravela.Framework.Impl.AspectOrdering
{
    internal interface IAspectOrderingSource
    {
        IEnumerable<AspectOrderSpecification> GetAspectOrderSpecification();
    }
}