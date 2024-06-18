#if TEST_OPTIONS
// @RequiredConstant(NET8_0_OR_GREATER)
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if NET8_0_OR_GREATER && ROSLYN_4_8_0_OR_GREATER
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Metalama.Framework.Advising; 
using Metalama.Framework.Aspects; 
using Metalama.Framework.Code;

// Having a custom InlineArrayAttribute that is RunTimeOrCompileTime still doesn't allow using it at compile-time
// (because it's considered system type and is removed from the compile-time compilation).
// This does not seem to be an important use-case, so it's probably not worth fixing.

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.InlineArrays_CompileTime_CustomAttribute
{

    public class TheAspect : OverrideMethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);

            var buffer = new Buffer();
            for (int i = 0; i < 10; i++)
            {
                buffer[i] = i;
            }

            foreach (var i in buffer)
            {
            }
        }

        public override dynamic? OverrideMethod()
        {
            var buffer2 = meta.CompileTime(new Buffer());
            foreach (int i in meta.CompileTime(Enumerable.Range(0, 10)))
            {
                buffer2[i] = i;
            }

            foreach (var i in buffer2)
            {
                Console.WriteLine(i);
            }

            return meta.Proceed();
        }
    }

    [RunTimeOrCompileTime]
#pragma warning disable CS0436 // Type conflicts with imported type
    [InlineArray(10)]
#pragma warning restore CS0436
    public struct Buffer
    {
        private int _element0;
    }

    public class C
    {
        [TheAspect]
        void M()
        {
        }
    }
}

namespace System.Runtime.CompilerServices
{
    [RunTimeOrCompileTime]
    internal class InlineArrayAttribute : Attribute
    {
        public InlineArrayAttribute(int size) { }
    }
}

#endif