using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfCompileTimeIfRunTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var p = meta.Parameters[0];
            if (string.Equals( meta.Method.Name, "NotNullMethod", StringComparison.Ordinal ))
            {
                if (p.Value == null)
                {
                    throw new ArgumentNullException(p.Name);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(p.Value))
                {
                    throw new ArgumentException("IsNullOrEmpty", p.Name);
                }
            }

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}