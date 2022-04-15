﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Issue30200
{
    public class MyAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            // The following line should generate a platform-specific type reference (because of the assembly reference),
            // but the platform should be ignored when resolving the reference.
            
            var isVoid = meta.Target.Method.ReturnType.Is(typeof(void));
            Console.WriteLine($"isVoid={isVoid}");
            return meta.Proceed();
        }
    }

    class Target
    {
        [MyAspect]
        void M() { }
    }
}