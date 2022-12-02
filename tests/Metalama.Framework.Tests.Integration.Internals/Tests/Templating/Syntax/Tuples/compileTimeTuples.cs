using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples1
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Tuple<string, int> tuple = meta.CompileTime(new Tuple<string, int>("string", 0));
            Console.WriteLine(tuple.Item1);
            
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}