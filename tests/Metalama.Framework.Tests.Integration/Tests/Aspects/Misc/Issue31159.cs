using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue31159;

[RunTimeOrCompileTime]
public class DerivedAspect : BaseAspect
{
    public override void Validate( dynamic? value )
    {
        Console.WriteLine( "Again" );
    }
}

// Target.
public interface I
{
    void M( [DerivedAspect] int x );
}

public class C : I
{
    public void M( [DerivedAspect] int x ) { }
}