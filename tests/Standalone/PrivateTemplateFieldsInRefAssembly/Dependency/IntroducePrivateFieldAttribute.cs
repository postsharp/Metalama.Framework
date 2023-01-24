using Metalama.Framework.Aspects;
using System;

namespace Dependency;

public class ErrAttribute : OverrideMethodAspect
{
    [Introduce]
    private readonly int _field;

    [Introduce]
    private void Method(int arg) {}

    [Introduce]
    private int AutoProperty { get; set; }

    [Introduce]
    internal int Property { get => 42; private set {} }

    [Introduce]
    private event EventHandler FieldLikeEvent;

    [Introduce]
    private event EventHandler Event { add {} remove {} }

    public override dynamic OverrideMethod()
    {
        Console.WriteLine(_field);

        return meta.Proceed();
    }
}

public class RunTimeOnlyClass { }