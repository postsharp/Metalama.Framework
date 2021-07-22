using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictOverrideBaseSealed
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int ExistingProperty
        {
            get => meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual int ExistingProperty
        {
            get => 13;
        }
    }

    internal class DerivedClass : BaseClass
    {
        public sealed override int ExistingProperty
        {
            get => 13;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : DerivedClass
    {
    }
}
