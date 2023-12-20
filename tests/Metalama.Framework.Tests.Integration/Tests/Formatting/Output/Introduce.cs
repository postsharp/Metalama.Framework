#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
# endif

using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0067, CS0168

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.Introduce
{
    public class IntroduceAspect : TypeAspect
    {
        [Introduce]
        public void Method()
        {
            Console.WriteLine( "Hello, world." );
        }

        [Introduce]
        public int Property { get; set; }

        [Introduce]
        public int PropertyWithBody
        {
            get
            {
                return 1;
            }
            set
            {
                Console.WriteLine( "Set" );
            }
        }

        [Introduce]
        public event EventHandler? Event;
    }

    [IntroduceAspect]
    public class TargetCode { }
}