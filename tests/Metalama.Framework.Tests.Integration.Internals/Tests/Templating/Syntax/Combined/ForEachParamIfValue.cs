using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Combined.ForEachParamIfValue
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            foreach (var p in meta.Target.Parameters)
            {
                if (p.Value == null)
                {
                    throw new ArgumentNullException(p.Name);
                }
            }

            dynamic? result = meta.Proceed();
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