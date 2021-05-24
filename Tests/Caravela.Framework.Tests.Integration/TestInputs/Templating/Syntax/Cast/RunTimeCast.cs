#pragma warning disable CS8600, CS8603
using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Cast.RunTimeCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            object arg0 = null;
            if (meta.Parameters.Count > 0)
            {
                arg0 = meta.Parameters[0].Value;
                if (arg0 is string)
                {
                    string s = (string)arg0;
                    Console.WriteLine(s);
                }
            }

            var result = meta.Proceed();
            object obj = result;
            string text = obj as string;
            if (text != null)
            {
                return text.Trim();
            }

            return obj;
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