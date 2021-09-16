using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.MultipleDeclarators_AllOverridden
{
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.OverrideEventAccessors(builder.Target.Events.OfName("A").Single(), nameof(OverrideAdd), nameof(OverrideRemove), null);
            builder.AdviceFactory.OverrideEventAccessors(builder.Target.Events.OfName("B").Single(), nameof(OverrideAdd), nameof(OverrideRemove), null);
            builder.AdviceFactory.OverrideEventAccessors(builder.Target.Events.OfName("C").Single(), nameof(OverrideAdd), nameof(OverrideRemove), null);
        }

        [Template]
        public void OverrideAdd(dynamic value)
        {
            Console.WriteLine("This is the add template.");
            meta.Proceed();
        }

        [Template]
        public void OverrideRemove(dynamic value)
        {
            Console.WriteLine("This is the remove template.");
            meta.Proceed();
        }
    }

    // <target>
    [Test]
    internal class TargetClass
    {
        public event EventHandler? A, B, C;
    }
}