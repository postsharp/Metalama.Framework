using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class OverrideMethodAspect : Attribute, IAspect<IMethod>
    {
        /// <inheritdoc />
        public virtual void Initialize( IAspectBuilder<IMethod> aspectBuilder )
        {
            var advice =
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, nameof( this.OverrideMethod ) );
        }

        /// <summary>
        /// Default template of the new method implementation.
        /// </summary>
        /// <returns></returns>
        [OverrideMethodTemplate]
        public abstract dynamic OverrideMethod();
    }
}
