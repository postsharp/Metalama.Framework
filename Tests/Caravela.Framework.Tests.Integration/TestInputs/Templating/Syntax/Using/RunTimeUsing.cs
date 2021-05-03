using System;
using System.IO;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.RunTimeUsing
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using (new MemoryStream())
            {
                var x = compileTime(0);
                var y = target.Parameters[0].Value + x;
                return proceed();
            }
            
            using ( MemoryStream s = new MemoryStream() )
            {
              Console.WriteLine("");
            }
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}