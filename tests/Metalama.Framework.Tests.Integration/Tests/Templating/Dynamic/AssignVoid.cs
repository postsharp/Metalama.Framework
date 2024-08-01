using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.AssignVoid
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var x = meta.Default( SpecialType.Int32 );

            x = meta.Proceed();

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