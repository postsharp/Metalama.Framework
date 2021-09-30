// @Skipped(https://tp.postsharp.net/entity/29144-linker-calls-to-a-method-of)

using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Introductions.Methods.GenericBaseType
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        [Introduce( WhenExists = OverrideStrategy.New )]
        public void Add(int value)
        {
            Console.WriteLine("Oops");
            meta.Proceed();
        }
    }

    // <target>
    [Aspect]
    class TargetCode : List<int>
    {
    
    }
}