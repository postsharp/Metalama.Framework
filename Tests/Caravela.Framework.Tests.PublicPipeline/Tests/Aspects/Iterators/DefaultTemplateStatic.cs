// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Iterators.DefaultTemplateStatic
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine($"Before {meta.Target.Member.Name}");
            var result = meta.Proceed();
            Console.WriteLine($"After {meta.Target.Member.Name}");
            return result;
            
        }
        
    }
    
    class Program
    {
        static void Main()
        {
            
            foreach ( var a in TargetCode.Enumerable(0) )
            {
                Console.WriteLine($" Received {a}");
            }
            
            foreach ( var a in TargetCode.OldEnumerable(0) )
            {
                Console.WriteLine($" Received {a}");
            }
            
            var enumerator1 = TargetCode.Enumerator(0);
            while ( enumerator1.MoveNext() )
            {
                Console.WriteLine($" Received {enumerator1.Current}");
            }
            
            var enumerator2 = TargetCode.OldEnumerator(0);
            while ( enumerator2.MoveNext() )
            {
                Console.WriteLine($" Received {enumerator2.Current}");
            }
            
        }
    }

    // <target>
    static class TargetCode
    {
        [Aspect]
        public static IEnumerable<int> Enumerable(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
        [Aspect]
        public static IEnumerator<int> Enumerator(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
        [Aspect]
        public static IEnumerable OldEnumerable(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
        
          [Aspect]
        public static IEnumerator OldEnumerator(int a)
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        }
      
    }
}