using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl
{
    internal sealed class AspectPartComparer : IComparer<AspectPart>
    {

        public int Compare( AspectPart x, AspectPart y ) => string.Compare( x.AspectType.Name, y.AspectType.Name, StringComparison.Ordinal );
    }
}
