using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class OverrideMethodAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, "Template" );
        }

        // TODO: somehow make the following code work?
        //[OverrideMethodTemplate]
        //public abstract object Template();
    }
}
