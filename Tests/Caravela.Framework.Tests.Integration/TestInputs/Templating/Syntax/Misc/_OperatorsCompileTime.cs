#pragma warning disable CS8600, CS8603
using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.OperatorsCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = target.Parameters.Count;

            i = +-i;
            i = unchecked(i + 1);
            i = checked(i - 1);
            unchecked { i++; }
            checked { i--; }
            ++i;
            --i;
            i += 1;
            i -= 1;
            i *= 1;
            i /= 1;
            i %= 3;
            i = i * 1;
            i = i / 1;
            i = i % 3;
            i ^= 1;
            i &= 2;
            i |= 2;
            i = i ^ 1;
            i = i & 2;
            i = i | 2;
            i <<= 1;
            i >>= 1;
            i = i << 1;
            i = i >> 1;
            i = ~(~i);

            bool x = i switch
            {
                1 => true,
                _ => false
            };
            bool y = (i >= 2);

            var t = (x, y);
            (x, y) = t;

            bool? z = ((x ^ y) && y) || !x;

            string s = default(string);
            s ??= "42";
            s = s[0..2];

            Console.WriteLine(i);
            Console.WriteLine(t);
            Console.WriteLine(z.Value);
            Console.WriteLine(s);
            Console.WriteLine(sizeof(bool));
            Console.WriteLine(typeof(int));

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        object Method(int a, int b)
        {
            return a + b;
        }
    }
}