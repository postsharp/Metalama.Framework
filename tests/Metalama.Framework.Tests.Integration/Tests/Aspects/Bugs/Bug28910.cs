using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug28910
{

    public class EmptyOverrideFieldOrPropertyAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed();
            set => meta.Proceed();
        }
    }

    // <target>
    class EmptyOverrideFieldOrPropertyExample
    {
        [EmptyOverrideFieldOrProperty]
        int _field;

    }
}