using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.TestApp.Aspects
{
    class InheritedAspect : TypeAspect
    {
        public override void BuildAspectClass(IAspectClassBuilder builder)
        {
            base.BuildAspectClass(builder);
            builder.IsInherited = true;
        }

        [Introduce]
        public void IntroducedMethod() { }
    }
}
