// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field )]
    public abstract class OverridePropertyAspect : Attribute, IAspect<IProperty>
    {
        /// <inheritdoc />
        public virtual void Initialize( IAspectBuilder<IFieldOrProperty> aspectBuilder )
        {
            aspectBuilder.AdviceFactory.OverrideProperty( aspectBuilder.TargetDeclaration, nameof( this.GetMethod ), nameof( this.SetMethod ) );
        }

        /// <summary>
        /// Default template of the new getter implementation.
        /// </summary>
        /// <returns></returns>
        [OverridePropertyGetTemplate]
        public abstract dynamic? GetMethod();

        /// <summary>
        /// Default template of the new setter implementation.
        /// </summary>
        /// <returns></returns>
        [OverridePropertySetTemplate]
        public abstract dynamic? SetMethod();
    }
}
