using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Field.Programmatic
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            {
                var introduced = builder.Advice.IntroduceField( builder.Target, "IntroducedField" );
                introduced.Type = TypeFactory.GetType( typeof(int) );
            }

            {
                var introduced = builder.Advice.IntroduceField( builder.Target, "IntroducedField_Static" );
                introduced.Type = TypeFactory.GetType( typeof(int) );
                introduced.IsStatic = true;
            }

            // TODO: Other members.
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}