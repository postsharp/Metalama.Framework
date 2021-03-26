#pragma warning disable CS8600, CS8603
using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Cast.RunTimeCast
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            object arg0 = null;
            if (target.Parameters.Count > 0)
            {
                arg0 = target.Parameters[0].Value;
                if (arg0 is string)
                {
                    string s = (string)arg0;
                    Console.WriteLine(s);
                }
            }

            var result = proceed();
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