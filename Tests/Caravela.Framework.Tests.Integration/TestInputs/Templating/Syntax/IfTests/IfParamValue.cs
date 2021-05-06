using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfParamValue
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (meta.Parameters[0].Value == null)
            {
                throw new ArgumentNullException(meta.Parameters[0].Name);
            }

            var p = meta.Parameters[1];
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