using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverrideDifferentReturnType
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int ExistingMethod()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual object? ExistingMethod()
        {
            return default;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass { }
}