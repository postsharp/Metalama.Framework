using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Combined.ForEachParamIfName
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            foreach (var p in meta.Target.Parameters)
            {
                if (p.Name.Length == 1)
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            foreach (var p in meta.Target.Parameters)
            {
                if (p.Name.StartsWith("b"))
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            dynamic? result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        string Method(object a, object bb)
        {
            return a.ToString() + bb.ToString();
        }
    }
}