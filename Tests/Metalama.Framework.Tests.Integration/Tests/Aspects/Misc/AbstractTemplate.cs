using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

// We are testing the abstract/override thing in templates.

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Misc.AbstractTemplate
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class AbstractAspect : Attribute, IAspect<IMethod>
    {
        public virtual void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod( builder.Target, nameof(OverrideMethod) );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IMethod> builder )
        {
            builder.ExceptForScenarios( EligibleScenarios.Inheritance ).MustBeNonAbstract();
        }

        [Template]
        public abstract dynamic? OverrideMethod();
    }

    public sealed class ConcreteAspect : AbstractAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "Override" );

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetCode
    {
        [ConcreteAspect]
        private int M() => 0;
    }
}