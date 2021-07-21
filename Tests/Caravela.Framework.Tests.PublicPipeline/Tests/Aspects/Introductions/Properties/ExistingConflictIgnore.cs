using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictIgnore
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public int ExistingProperty
        {
            get => 42;
        }

        [Introduce(WhenExists = OverrideStrategy.Ignore)]
        public static int ExistingProperty_Static
        {
            get => 42;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public int ExistingProperty
        {
            get => 27;
        }

        public static int ExistingProperty_Static
        {
            get => 27;
        }
    }
}
