using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class OverrideMethodAspect : Attribute, IAspect<IMethod>
    {
        public void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, nameof( Template ) );
        }

        [OverrideMethodTemplate]
        public abstract dynamic Template();
    }
}
