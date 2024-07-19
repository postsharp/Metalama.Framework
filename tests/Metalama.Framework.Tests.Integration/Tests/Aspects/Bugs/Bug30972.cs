using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug30972
{
    public class TestAspect : OverrideFieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            base.BuildAspect( builder );
        }

        public override dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine( "Aspect" );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "Aspect" );
                meta.Proceed();
            }
        }
    }

    public class FieldsFabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender.SelectMany( p => p.Types.SelectMany( t => t.Fields ) ).AddAspect<TestAspect>();
        }
    }

    // <target>
    public class TargetClass
    {
        public const int X = 42;

        public int Control;
    }

    // <target>
    public enum TargetEnum
    {
        X = 42
    }
}