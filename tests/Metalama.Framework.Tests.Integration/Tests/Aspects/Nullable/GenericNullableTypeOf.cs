using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.GenericNullableTypeOf
{
#pragma warning disable CS8600, CS0169

    internal class MyAspect : FieldOrPropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advice.IntroduceMethod(builder.Target.DeclaringType, nameof(Template), args: new { T = builder.Target.Type });
        }

        [Template]
        public void Template<[CompileTime]T>()
        {
            GlobalServiceProvider serviceProvider = null!;
            var x = (T) serviceProvider.GetService(typeof(T));
        }
    }

    // <target>
    internal class C
    {
        [MyAspect]
        string? _f;
    }


}
