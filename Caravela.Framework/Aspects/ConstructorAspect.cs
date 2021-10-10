// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Constructor )]
    public abstract class ConstructorAspect : Attribute, IAspect<IConstructor>
    {
        public virtual void BuildAspect( IAspectBuilder<IConstructor> builder ) { }

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

        public virtual void BuildEligibility( IEligibilityBuilder<IConstructor> builder ) { }
    }
}