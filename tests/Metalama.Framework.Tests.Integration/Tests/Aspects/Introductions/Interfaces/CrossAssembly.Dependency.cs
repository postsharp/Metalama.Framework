using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.CrossAssembly
{
    /*
     * Tests that async methods, iterators and async iterators are correctly introduced.
     */

    public interface IInterface 
    {
        T Method<T>(T x);

        int Property { get; set; }

        int AutoProperty { get; set; }

        event EventHandler Event;

        event EventHandler? EventField;

        Task<int> AsyncMethod();

        IEnumerable<int> Iterator();

        IAsyncEnumerable<int> AsyncIterator();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [Introduce]
        public T Method<T>(T x)
        {
            Console.WriteLine("Introduced");
            return x;
        }

        [Introduce]
        public int Property
        { 
            get
            {
                Console.WriteLine("Introduced");
                return 42;
            }

            set
            {
                Console.WriteLine("Introduced");
            }
        }

        [Introduce]
        public int AutoProperty { get; set; }

        [Introduce]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("Introduced");
            }

            remove
            {
                Console.WriteLine("Introduced");
            }
        }

        [Introduce]
        public event EventHandler? EventField;

        [Introduce]
        public async Task<int> AsyncMethod()
        {
            Console.WriteLine("Introduced");
            await Task.Yield();
            return 42;
        }

        [Introduce]
        public IEnumerable<int> Iterator()
        {
            Console.WriteLine("Introduced");
            yield return 42;
        }

        [Introduce]
        public async IAsyncEnumerable<int> AsyncIterator()
        {
            Console.WriteLine("Introduced");
            await Task.Yield();
            yield return 42;
        }
    }
}