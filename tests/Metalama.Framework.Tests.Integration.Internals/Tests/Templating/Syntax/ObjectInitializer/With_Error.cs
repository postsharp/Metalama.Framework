using System.Collections.Generic;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.With_Error
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var compileTime1 = meta.CompileTime(new Entity1 { Property1 = 5 });

            // This should result in an error.
            var error = compileTime1 with { Property1 = meta.This.Foo };

            return default;
        }
    }

    [RunTimeOrCompileTime]
    internal record Entity1
    {
        public int Property1 { get; set; }
    }


    internal class TargetCode
    {
        private object Method( object a )
        {
            return a;
        }
    }
}