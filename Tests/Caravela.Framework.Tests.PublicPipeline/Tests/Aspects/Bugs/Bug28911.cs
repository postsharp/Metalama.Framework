using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug18911
{
 public class EmptyOverrideFieldOrPropertyAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.Proceed();
            set => meta.Proceed();
        }
    }

    // <target>
   class EmptyOverrideFieldOrPropertyExample
    {
        [EmptyOverrideFieldOrProperty]
        public string? Property { get; set; }
    }
}