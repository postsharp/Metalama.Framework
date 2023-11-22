#if TEST_OPTIONS
// @KeepDisabledCode
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug31077
{
    public class TestAspect : MethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Advice.Override(builder.Target, nameof(OverrideMethod));
        }

        [Template]
        public dynamic? OverrideMethod()
        {
            _ = meta.Proceed();
            return meta.Proceed();
        }
    }

    // <target>
    public class TargetClass<T> : IEnumerable<T>
    {
        [TestAspect]
        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Empty<T>().GetEnumerator();
        }

        [TestAspect]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
