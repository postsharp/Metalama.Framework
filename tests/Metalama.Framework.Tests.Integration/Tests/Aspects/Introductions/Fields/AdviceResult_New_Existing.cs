﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using Metalama.Framework.Advising;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Fields.AdviceResult_New_Existing
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var result = builder.Advice.IntroduceField( builder.Target, nameof(Field), whenExists: OverrideStrategy.New );

            if (result.Outcome != AdviceOutcome.Error)
            {
                throw new InvalidOperationException( $"Outcome was {result.Outcome} instead of Error." );
            }

            if (result.AdviceKind != AdviceKind.IntroduceField)
            {
                throw new InvalidOperationException( $"AdviceKind was {result.AdviceKind} instead of IntroduceField." );
            }

            // TODO: #33060
            //if (!builder.Target.Compilation.Comparers.Default.Equals(
            //        result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
            //        builder.Target.Fields.Single()))
            //{
            //    throw new InvalidOperationException($"Declaration was not correct.");
            //}
        }

        [Template]
        public int Field;
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public int Field;
    }
}