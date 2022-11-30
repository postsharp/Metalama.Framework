#pragma warning disable CS0162

using System;
using System.IO;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Using.RunTimeUsing
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            using (new MemoryStream())
            {
                var x = meta.CompileTime(0);
                var y = meta.Target.Parameters[0].Value + x;
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