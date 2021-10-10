// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.GenericParameter )]
    public abstract class TypeParameterAspect : Aspect, IAspect<ITypeParameter>
    {
        public virtual void BuildAspect( IAspectBuilder<ITypeParameter> builder ) { }

        public virtual void BuildEligibility( IEligibilityBuilder<ITypeParameter> builder ) { }
    }
}