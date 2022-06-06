using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.EnumSerialization
{
    class LogAttribute : OverrideMethodAspect
{
    // Template that overrides the methods to which the aspect is applied.
    public override dynamic? OverrideMethod()
    {
        var color = meta.CompileTime( ConsoleColor.Blue );
        
       Console.ForegroundColor = color;
       
       return meta.Proceed();
    }
}


// <target>
    class TargetCode
    {
        [LogAttribute]
        int Method(int a)
        {
            return a;
        }
    }
}