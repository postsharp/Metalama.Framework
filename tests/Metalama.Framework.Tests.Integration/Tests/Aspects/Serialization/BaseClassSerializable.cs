﻿using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable;

/*
 * The serializable base class of a serializable type.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}