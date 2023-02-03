using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.ParameterMapping
{
    /*
     * Verifies that template parameters are correctly mapped.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(
                builder.Target.Methods.OfName("Method_InvertedParameters").Single(),
                nameof(InvertedParameters) );
        }

        [Template]
        public int InvertedParameters(int x, string y)
        {
            var z = meta.Proceed();
            return x + y.Length;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass 
    {
        public int Method_InvertedParameters(string y,int x)
        {
            return x + y.Length;
        }
    }
}