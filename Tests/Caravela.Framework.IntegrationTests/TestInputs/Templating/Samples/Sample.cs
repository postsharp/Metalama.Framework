using System;
using System.Text;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Templating.Samples.Sample
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            var parameters = new object[target.Parameters.Count];
            var stringBuilder = compileTime(new StringBuilder());
            stringBuilder.Append(target.Method.Name);
            stringBuilder.Append('(');
            int i = compileTime(0);
            foreach (var p in target.Parameters)
            {
                string comma = i > 0 ? ", " : "";

                if (p.IsOut())
                {
                    stringBuilder.Append($"{comma}{p.Name} = <out> ");
                }
                else
                {
                    stringBuilder.Append($"{comma}{p.Name} = {{{i}}}");
                    parameters[i] = p.Value;
                }

                i++;
            }
            stringBuilder.Append(')');

            Console.WriteLine(stringBuilder.ToString(), parameters);

            try
            {
                dynamic result = proceed();
                Console.WriteLine(stringBuilder + " returned " + result, parameters);
                return result;
            }
            catch (Exception _e)
            {
                Console.WriteLine(stringBuilder + " failed: " + _e, parameters);
                throw;
            }
        }
    }

    internal class TargetCode
    {
        private int Method(int a, int b, out int c)
        {
            c = a - b;
            return a + b;
        }
    }
}