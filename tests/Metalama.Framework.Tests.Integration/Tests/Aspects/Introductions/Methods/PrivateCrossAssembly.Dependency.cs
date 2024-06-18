﻿using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.PrivateCrossAssembly;

public class MyAspect : TypeAspect
{
    [Introduce]
    private InternalClass IntroducedMethod() => new();
}

internal class InternalClass { }