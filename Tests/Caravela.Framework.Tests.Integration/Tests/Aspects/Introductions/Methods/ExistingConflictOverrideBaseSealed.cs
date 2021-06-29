using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.ExistingConflictOverrideBaseSealed
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
       

       
        
        
        [Introduce(ConflictBehavior = ConflictBehavior.Override)]
        public int ExistingMethod()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual void ExistingMethod()
        {
        }
    }

    internal class DerivedClass : BaseClass
    {
        public sealed override void ExistingMethod()
        {
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : DerivedClass
    {
    }
}
