using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Advices
{
    internal interface IAdviceImplementation
    {
        AdviceResult ToResult( ICompilation compilation );
    }
}
