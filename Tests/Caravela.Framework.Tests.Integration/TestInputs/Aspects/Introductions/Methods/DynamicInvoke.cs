using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DynamicInvoke
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public void IntroduceVoid()
        {
            Console.WriteLine("Introduced");
            meta.Method.Invoke();
        }
        
        [Introduce]
        public int IntroduceInt()
        {
            Console.WriteLine("Introduced");
                        
            return meta.Method.Invoke();
        }
        
         [Introduce(ConflictBehavior = ConflictBehavior.Override)]
        public int OverrideInt()
        {
            Console.WriteLine("Introduced");
            
            // TODO: This produces an incorrect result.
            return meta.Method.Invoke();
        }
        
          [Introduce(ConflictBehavior = ConflictBehavior.Override)]
        public void OverrideVoid()
        {
            Console.WriteLine("Introduced");
            
            // TODO: This produces an incorrect result.
            meta.Method.Invoke();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
        public int OverrideInt() 
        {
            return 1;
        }
        
        public void OverrideVoid()
        {
        }
        
    }
}
