using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfParamValue
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if (meta.Target.Parameters[0].Value == null)
            {
                throw new ArgumentNullException(meta.Target.Parameters[0].Name);
            }

            var p = meta.Target.Parameters[1];
            if (p.Value == null)
            {
                throw new ArgumentNullException(p.Name);
            }

            return meta.Proceed();
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