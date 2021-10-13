using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

// We are testing the abstract/override thing in templates.

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Misc.AbstractTemplate
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
            builder.ExceptForInheritance().MustBeNonAbstract();
        }

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

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