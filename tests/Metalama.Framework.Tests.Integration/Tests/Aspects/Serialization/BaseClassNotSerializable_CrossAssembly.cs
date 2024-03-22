﻿using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassNotSerializable_CrossAssembly;

/*
 * The base class of a serializable type defined in another assembly is not itself serializable and has a parameterless base constructor.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}