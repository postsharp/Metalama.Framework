#pragma warning disable CS0162

using System;
using System.IO;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Using.RunTimeUsing
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            using (new MemoryStream())
            {
                var x = meta.CompileTime(0);
                var y = meta.Parameters[0].Value + x;
                return meta.Proceed();
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