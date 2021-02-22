using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.Combined.ForEachParamIfName
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            foreach (var p in target.Parameters)
            {
                if (p.Name.Length == 1)
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            foreach (var p in target.Parameters)
            {
                if (p.Name.StartsWith("b"))
                {
                    Console.WriteLine("{0} = {1}", p.Name, p.Value);
                }
            }

            dynamic result = proceed();
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