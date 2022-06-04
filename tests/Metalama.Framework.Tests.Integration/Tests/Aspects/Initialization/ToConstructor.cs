using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.ToConstructor;

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect( IAspectBuilder<IConstructor> builder )
    {
        builder.Advice.AddInitializer( builder.Target, nameof(Initialize) );
    }

    [Introduce]
    private int _f;

    [Template]
    private void Initialize()
    {
        _f = 5;
    }
}

// <target>
public class C
{
    [MyAspect]
    public C() { }
}