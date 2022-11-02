using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metalama.Framework.Code.SyntaxBuilders;

#pragma warning disable CS1717, CS0414

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.FieldOrPropertyValueShortcuts_BackingField
{
    internal class MyAspect : TypeAspect
    {
        [Introduce]
        public void Method()
        {
            foreach (var field in meta.Target.Type.Fields)
            {
                field.ToExpression().Value = field.ToExpression().Value;
            }
        }
    }

    // <target>
    [MyAspect]
    internal class C
    {
        public int P { get; set; }
    }
}