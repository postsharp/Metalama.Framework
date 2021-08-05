// @IgnoredDiagnostic(CS1998)

using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Templating.Aspects.Iterators.DefaultTemplateGetter
{
    class Aspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
        get {
            Console.WriteLine($"Before {meta.Target.Member.Name}");
            var result = meta.Proceed();
            Console.WriteLine($"After {meta.Target.Member.Name}");
            return result;
        }
        set => throw new NotImplementedException(); 
        }
        
    }
    
    class Program
    {
        static void Main()
        {
            TargetCode targetCode = new();
            
            foreach ( var a in targetCode.Enumerable )
            {
                Console.WriteLine($" Received {a}");
            }
            
            foreach ( var a in targetCode.OldEnumerable )
            {
                Console.WriteLine($" Received {a}");
            }
            
            var enumerator1 = targetCode.Enumerator;
            while ( enumerator1.MoveNext() )
            {
                Console.WriteLine($" Received {enumerator1.Current}");
            }
            
            var enumerator2 = targetCode.OldEnumerator;
            while ( enumerator2.MoveNext() )
            {
                Console.WriteLine($" Received {enumerator2.Current}");
            }
            
        }
    }

    // <target>
    class TargetCode
    {
        [Aspect]
        public IEnumerable<int> Enumerable { get 
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        } }
        
        [Aspect]
        public IEnumerator<int> Enumerator { get
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        } }
        
        [Aspect]
        public IEnumerable OldEnumerable { get 
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        } }
        
          [Aspect]
        public IEnumerator OldEnumerator { get
        {
            Console.WriteLine("Yield 1");
            yield return 1;
            Console.WriteLine("Yield 2");
            yield return 2;
            Console.WriteLine("Yield 3");
            yield return 3;
        } }
      
    }
}