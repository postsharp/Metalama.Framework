using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.RunTimeOnlyType;

public class Aspect : TypeAspect
{
    [Introduce]
    internal event EventHandler<RunTimeClass>? FieldLikeEvent;

    [Introduce]
    internal event EventHandler<RunTimeClass> Event
    {
        add { }
        remove { }
    }
}

// <target>
[Aspect]
internal class RunTimeClass { }