﻿using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Serialization.BaseClassSerializable_CrossAssembly;

/*
 * The serializable base class of a serializable type defined in another assembly.
 */

//<target>
public class TargetClass : BaseClass
{
    public override void Foo()
    {
        Console.WriteLine("Original");
    }
}