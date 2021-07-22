using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictOverrideBaseNonVirtual
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int BaseProperty
        {
            get => meta.Proceed();
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public static int BaseProperty_Static
        {
            get => meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public int BaseProperty
        {
            get => 13;
        }

        public static int BaseProperty_Static
        {
            get => 13;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
    }
}
