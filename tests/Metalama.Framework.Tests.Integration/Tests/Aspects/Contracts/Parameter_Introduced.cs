// @Skipped(#30612 - ctor parameter)

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Parameter_Introduced
{
    // Tests that a contract on introduced ctor parameter work properly.

    internal class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.Advice.IntroduceParameter(
                    constructor,
                    "dependency",
                    typeof(int),
                    TypedConstant.Create(0));

                foreach (var parameter in constructor.ForCompilation(builder.Advice.MutableCompilation).Parameters)
                {
                    builder.Advice.AddContract(parameter, nameof(Validate));
                }
            }
        }


        [Template]
        public void Validate( int value )
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException( meta.Target.Parameter.Name );
            }
        }
    }

    // <target>
    [Test]
    internal class Target
    {
        public Target()
        {
        }

        public Target(int x)
        {
        }
    }
}