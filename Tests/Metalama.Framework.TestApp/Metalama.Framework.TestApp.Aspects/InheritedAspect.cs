using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.TestApp.Aspects
{
    [Inherited]
    class InheritedAspect : TypeAspect
    {
        [Introduce]
        public void IntroducedMethod() { }
    }
}
