using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.ExistingConflictOverrideDifferentReturnType
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
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
    internal class TargetClass : BaseClass { }
}