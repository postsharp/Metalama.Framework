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
    public abstract class OverrideFieldOrPropertyAspect : Attribute, IAspect<IFieldOrProperty>
    {
        /// <inheritdoc />
        public virtual void Initialize( IAspectBuilder<IFieldOrProperty> aspectBuilder )
        {
            aspectBuilder.AdviceFactory.OverrideFieldOrProperty( aspectBuilder.TargetDeclaration, nameof( this.OverrideProperty ) );
        }

        [OverrideFieldOrPropertyTemplate]
        public abstract dynamic? OverrideProperty
        {
            get; set;
        }
    }
}
