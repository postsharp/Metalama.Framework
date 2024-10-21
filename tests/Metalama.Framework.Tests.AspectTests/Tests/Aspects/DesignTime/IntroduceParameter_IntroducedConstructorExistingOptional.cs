#if TEST_OPTIONS
// @TestScenario(DesignTime)
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_IntroducedConstructorExistingOptional;

[assembly: AspectOrder( AspectOrderDirection.CompileTime, typeof(ConstructorIntroductionAttribute), typeof(ParameterIntroductionAttribute) )]

/*
 * Tests that when a parameter is appended to an introduced constructor and there already is an existing options constructor, the design-time
 * pipeline generates a correct constructor with optional parameters.
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_IntroducedConstructorExistingOptional;

public class ConstructorIntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introduced = builder.IntroduceConstructor(
            nameof(Template),
            buildConstructor: c => { c.AddParameter( "x", typeof(int) ); } );

        builder.With( introduced.Declaration ).Outbound.AddAspect<ParameterIntroductionAttribute>();
    }

    [Template]
    public void Template() { }
}

public class ParameterIntroductionAttribute : ConstructorAspect
{
    public override void BuildAspect( IAspectBuilder<IConstructor> builder )
    {
        builder.IntroduceParameter( "introduced1", typeof(int), TypedConstant.Create( 42 ) );
        builder.IntroduceParameter( "introduced2", typeof(string), TypedConstant.Create( "42" ) );
    }
}

// <target>
[ConstructorIntroduction]
internal partial class TestClass
{
    [ParameterIntroduction]
    public TestClass( int param, int optParam = 42 ) { }
}