// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base class for aspects that can be applied to type parameters (i.e. generic parameters) as custom attributes.
    /// </summary>
    /// <remarks>
    /// <para>This class is a redundant helper class. The aspect framework only respects the <see cref="IAspect{T}"/> interface.</para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.GenericParameter )]
    public abstract class TypeParameterAspect : Aspect, IAspect<ITypeParameter>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<ITypeParameter> builder ) { }

        /// <inheritdoc />
        public virtual void BuildEligibility( IEligibilityBuilder<ITypeParameter> builder ) { }
    }
}