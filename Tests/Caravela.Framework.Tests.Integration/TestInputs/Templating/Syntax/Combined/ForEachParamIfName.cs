using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Combined.ForEachParamIfName
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            foreach (var p in meta.Parameters)
            {
                if (p.Name.Length == 1)
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            foreach (var p in meta.Parameters)
            {
                if (p.Name.StartsWith("b"))
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            dynamic result = meta.Proceed();
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