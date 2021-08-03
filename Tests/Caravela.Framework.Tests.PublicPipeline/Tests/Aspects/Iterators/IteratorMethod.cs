// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Iterators.NormalTemplateOnIteratorMethod
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Before");
            var result = meta.Proceed();
            Console.WriteLine("After");
            return result;
            
        }
        
    }

    // <target>
    class TargetCode
    {
        [Aspect]
        IEnumerable<int> Enumerable(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
        [Aspect]
        IEnumerator<int> Enumerator(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
        [Aspect]
        IEnumerable OldEnumerable(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
        
          [Aspect]
        IEnumerator OldEnumerator(int a)
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }
      
    }
}