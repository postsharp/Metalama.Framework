// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            aspectBuilder.AdviceFactory.OverrideMethod( aspectBuilder.TargetDeclaration, nameof( this.OverrideMethod ) );
        }

        /// <summary>
        /// Default template of the new method implementation.
        /// </summary>
        /// <returns></returns>
        [OverrideMethodTemplate]
        public abstract dynamic? OverrideMethod();
    }
}