﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.ParameterMapping
{
    /*
     * Verifies that template parameters are correctly mapped by name.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceMethod(
                builder.Target,
                nameof(InvertedParameterNames),
                buildMethod: b =>
                {
                    b.Parameters[0].Name = "y";
                    b.Parameters[1].Name = "x";
                    b.AddParameter("z", typeof(int));
                } );
        }

        [Template]
        public int InvertedParameterNames(int x, string y)
        {
            return x + y.Length;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}