using System;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Throw.ThrowExpressionBody
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template() => throw new Exception();
    }

    internal class TargetCode
    {
        private void Method( int a )
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}