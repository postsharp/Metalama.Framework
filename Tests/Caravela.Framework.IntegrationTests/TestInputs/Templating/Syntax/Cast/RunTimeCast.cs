#pragma warning disable CS8600, CS8603
using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.Cast.RunTimeCast
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private string Method(string a)
        {
            return a;
        }
    }
}