using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Aspects
{
    public interface IAspect
    {
        void Initialize(IAspectBuilder aspectBuilder);
    }

    public interface IAspectBuilder { }
}
