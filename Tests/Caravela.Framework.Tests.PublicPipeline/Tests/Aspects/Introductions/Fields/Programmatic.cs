using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Field.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            {
                var introduced = builder.AdviceFactory.IntroduceField(builder.TargetDeclaration, "IntroducedField");
                introduced.Type = introduced.Compilation.TypeFactory.GetTypeByReflectionType(typeof(int));
            }

            {
                var introduced = builder.AdviceFactory.IntroduceField(builder.TargetDeclaration, "IntroducedField_Static");
                introduced.Type = introduced.Compilation.TypeFactory.GetTypeByReflectionType(typeof(int));
                introduced.IsStatic = true;
            }

            // TODO: Other members.
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
