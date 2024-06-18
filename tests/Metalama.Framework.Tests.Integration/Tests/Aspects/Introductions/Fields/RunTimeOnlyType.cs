using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Fields.RunTimeOnlyType;

public class Aspect : TypeAspect
{
    [Introduce]
    internal RunTimeClass? Event;
}

// <target>
[Aspect]
internal class RunTimeClass { }