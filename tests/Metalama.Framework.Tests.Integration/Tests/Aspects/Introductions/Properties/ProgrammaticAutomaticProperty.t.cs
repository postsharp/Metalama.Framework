// Warning CS8602 on `property3.SetMethod`: `Dereference of a possibly null reference.`
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AutomaticProperty;
#pragma warning disable CS0067

public class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

[MyAspect]
public class C {

private global::System.Int32 _p1;


private global::System.Int32 P1 
{ get
{ 
        return this._p1;
}
set
{ 
        this._p1=value;
}
}

private global::System.Int32 _p2;


protected global::System.Int32 P2 
{ get
{ 
        return this._p2;
}
set
{ 
        this._p2=value;
}
}

private global::System.Int32 _p3;


public global::System.Int32 P3 
{ get
{ 
        return this._p3;
}
protected set
{ 
        this._p3=value;
}
}}
