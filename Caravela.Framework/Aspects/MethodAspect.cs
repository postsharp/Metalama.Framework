// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class MethodAspect : Attribute, IAspect<IMethod>
    {
        public virtual void BuildAspect( IAspectBuilder<IMethod> builder ) { }

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

        public virtual void BuildEligibility( IEligibilityBuilder<IMethod> builder ) { }
    }
}