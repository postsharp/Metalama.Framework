using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.ObjectInitializers
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var a = new Entity1
            {
                Property1 = 1,
                Property2 =
            {
                new Entity2 { Property1 = 2 },
                new Entity2 { Property1 = 3}
            }
            };

            var b = a with { Property1 = 2 };

            dynamic? result = meta.Proceed();
            return result;
        }
    }

    record Entity1
    {
        public int Property1 { get; set; }

        public IList<Entity2> Property2 { get; set; } = new List<Entity2>();
    }

    struct Entity2
    {
        public int Property1 { get; set; }
    }

    class TargetCode
    {
        object Method(object a)
        {
            return a;
        }
    }
}