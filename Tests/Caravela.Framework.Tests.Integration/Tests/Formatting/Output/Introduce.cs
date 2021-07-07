using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using PostSharp.Aspects;

#pragma warning disable CS8618, CS0067, CS0168

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.Introduce
{
    public class IntroduceAspect : Attribute, IAspect<INamedType>
    {
        
        [Introduce]
        public void Method()
        {
            Console.WriteLine("Hello, world.");
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
                Console.WriteLine("Set");
            }
        }

        [Introduce]
        public event EventHandler Event;
    }

    [IntroduceAspect]
    public class TargetCode
    {
        
    }
}