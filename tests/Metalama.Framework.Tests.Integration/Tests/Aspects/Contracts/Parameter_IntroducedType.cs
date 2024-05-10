using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Parameter_IntroducedType
{
    internal class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introducedType =
                builder.Advice.IntroduceType(
                    builder.Target,
                    "IntroducedType",
                    TypeKind.Class,
                    buildType: b => 
                    { 
                        b.Accessibility = Accessibility.Public; 
                    })
                .Declaration;

            // TODO: It's now necessary to translate the introduced type.

            var introducedMethod =
                builder.Advice.IntroduceMethod(
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation),
                    nameof(IntroducedMethodTemplate),
                    buildMethod: b =>
                    {
                        b.AddParameter("p", introducedType);
                    })
                .Declaration;

            builder.Advice.AddContract(introducedMethod.Parameters.Single(), nameof(ValidateTemplate));
        }

        [Template]
        public void IntroducedMethodTemplate()
        {
        }

        [Template]
        public void ValidateTemplate( dynamic? value )
        {
            if (value == null)
            {
                throw new ArgumentNullException( meta.Target.Parameter.Name );
            }
        }
    }

    // <target>
    [Test]
    internal class Target
    {
    }
}