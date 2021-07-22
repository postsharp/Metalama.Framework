using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictOverrideDifferentReturnType
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
        public virtual object? ExistingProperty
        {
            get => default;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
    }
}
