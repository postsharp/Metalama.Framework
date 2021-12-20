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
        public int IntroducedProperty_Accessors { get; set; } = meta.Target.Type.Name.Length;
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}
