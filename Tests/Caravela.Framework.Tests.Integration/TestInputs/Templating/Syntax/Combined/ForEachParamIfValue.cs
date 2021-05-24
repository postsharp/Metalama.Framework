using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Combined.ForEachParamIfValue
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            foreach (var p in meta.Parameters)
            {
                if (p.Value == null)
                {
                    throw new ArgumentNullException(p.Name);
                }
            }

            dynamic result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        string Method(object a, object b)
        {
            return a.ToString() + b.ToString();
        }
    }
}