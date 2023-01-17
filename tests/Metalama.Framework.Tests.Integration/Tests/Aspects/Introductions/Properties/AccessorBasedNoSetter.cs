using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AccessorBasedNoSetter
{
    internal class MyAspect : TypeAspect
    {
        [Template]
        public int Getter() => 5;

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceProperty( builder.Target, "TheProperty", nameof(Getter), null );
        }
    }

    // <target>
    [MyAspect]
    internal class C { }
}