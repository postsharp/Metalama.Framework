using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax.Misc.CompileTimeThis
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {

            Console.WriteLine(CompileTimeMethod( this ));
            
            return 0;
        }

        static string CompileTimeMethod( Aspect a ) => a.ToString()!;

    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}