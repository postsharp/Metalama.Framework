using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.Declarative_Async
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public async void IntroducedMethod_Void()
        {
            Console.WriteLine( "This is introduced method." );
            await Task.Yield();
            meta.Proceed();
        }

        [Introduce]
        public async Task IntroducedMethod_TaskVoid()
        {
            Console.WriteLine("This is introduced method.");
            await Task.Yield();
            meta.Proceed();
        }

        [Introduce]
        public async Task<int> IntroducedMethod_TaskInt()
        {
            Console.WriteLine( "This is introduced method." );
            await Task.Yield();
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}