using System;
using System.Linq;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.TypeOfBug
{
   
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
//[CompileTime] // TODO: should not be necessary to add [CompileTime]
public class NotToStringAttribute : Attribute { }
#pragma warning disable CS0067


public class ToStringAttribute : TypeAspect
{
 

    [Introduce]
public string IntroducedToString() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067


    [ToString]
    class TargetCode
    {


public global::System.String IntroducedToString()
{
    global::System.Console.WriteLine(typeof(global::Metalama.Framework.Tests.Integration.Aspects.Misc.TypeOfBug.NotToStringAttribute));
    return "NotToStringAttribute";
}        
    }
}
