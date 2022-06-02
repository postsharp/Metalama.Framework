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


public class ToStringAttribute : TypeAspect
{
 

    [Introduce]
    public string IntroducedToString()
    {
        var t = meta.CompileTime( typeof(NotToStringAttribute) );
        var n = meta.CompileTime( nameof(NotToStringAttribute) );
        Console.WriteLine( t );
        return n;
     
    }
}


    [ToString]
    class TargetCode
    {
        
    }
}