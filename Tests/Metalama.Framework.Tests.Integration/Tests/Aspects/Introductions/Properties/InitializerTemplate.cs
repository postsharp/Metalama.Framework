using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.InitializerTemplate
{
    public class Introduction : TypeAspect
    {
        [Introduce]
        public string IntroducedProperty { get; set; } = meta.Target.Member.Name;

        [Introduce]
        public static string IntroducedProperty_Static { get; set; } = meta.Target.Member.Name;
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}
