using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.AssignVoid_CompoundError
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var x = TypeFactory.GetType( SpecialType.Int32 ).DefaultValue();

            x += meta.Proceed();
            x *= meta.Proceed();

            return default;
        }
    }

    internal class TargetCode
    {
        private void Method( int a )
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}