using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfCompileTimeIfRunTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var p = meta.Target.Parameters[0];
            if (string.Equals( meta.Target.Method.Name, "NotNullMethod", StringComparison.Ordinal ))
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