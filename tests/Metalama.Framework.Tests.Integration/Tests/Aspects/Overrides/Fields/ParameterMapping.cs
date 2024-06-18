﻿using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.ParameterMapping
{
    /*
     * Verifies that template parameter is correctly mapped by index.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.OverrideAccessors(
                builder.Target.Fields.Single(),
                null,
                nameof(RenamedValueParameter) );
        }

        [Template]
        public void RenamedValueParameter( int x )
        {
            x = 42;
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public int Value;
    }
}