﻿using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32574;

[Inheritable]
public class TheAspect : TypeAspect
{
    [Introduce]
    private void TheMethod()
    {
        Console.WriteLine( $"{meta.AspectInstance.SecondaryInstances.Length + 1} aspect instance(s)." );
    }
}

[TheAspect]
public class C1 { }