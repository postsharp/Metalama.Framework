using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.ParameterMapping
{
    /*
     * Verifies that template parameters are correctly mapped by name.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(
                builder.Target.Methods.OfName("Method_InvertedParameters").Single(),
                nameof(InvertedParameters) );

            builder.Advice.Override(
                builder.Target.Methods.OfName("Method_SelectFirstParameter").Single(),
                nameof(SelectFirstParameter));

            builder.Advice.Override(
                builder.Target.Methods.OfName("Method_SelectSecondParameter").Single(),
                nameof(SelectSecondParameter));
        }

        [Template]
        public int InvertedParameters(int y, string x)
        {
            var z = meta.Proceed();
            return x.Length + y;
        }

        [Template]
        public int SelectFirstParameter(string x)
        {
            var z = meta.Proceed();
            return x.Length;
        }

        [Template]
        public int SelectSecondParameter(int y)
        {
            var z = meta.Proceed();
            return y;
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass 
    {
        public int Method_InvertedParameters(string x, int y)
        {
            return x.Length + y;
        }
        public int Method_SelectFirstParameter(string x, int y)
        {
            return x.Length + y;
        }
        public int Method_SelectSecondParameter(string x, int y)
        {
            return x.Length + y;
        }
    }
}