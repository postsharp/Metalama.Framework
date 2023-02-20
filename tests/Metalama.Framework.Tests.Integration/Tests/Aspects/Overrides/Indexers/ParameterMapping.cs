using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.ParameterMapping
{
    /*
     * Verifies that template parameter is correctly mapped by index.
     */

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.OverrideAccessors(
                builder.Target.Indexers.Single(),
                nameof(RenamedParameterGetTemplate),
                nameof(RenamedParameterSetTemplate));
        }

        [Template]
        public int RenamedParameterGetTemplate(int y, string x)
        {
            var q = y + x.ToString().Length;
            return meta.Proceed();
        }

        [Template]
        public void RenamedParameterSetTemplate(int y, string x, int z)
        {
            var q = y + x.ToString().Length + z;
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass 
    {
        public int this[int x, string y]
        {
            get
            {
                return x + y.ToString().Length;
            }

            set
            {
                var z = x + y.ToString() + value;
            }
        }
    }
}