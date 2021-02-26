using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Overrides.Methods.Declarative
{
    public class OverrideAttribute : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
        }

        [OverrideMethod]
        public dynamic Template()
        {
            Console.WriteLine( "This is the overriding method." );
            return proceed();
        }
    }

    #region Target
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine( "This is the original method." );
        }

        [Override]
        public void TargetMethod_Void(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
        }

        [Override]
        public int TargetMethod_Int()
        {
            Console.WriteLine("This is the original method.");
            return 42;
        }

        [Override]
        public int TargetMethod_Int(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
        }
    }
    #endregion
}
