// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base class for aspects that can be applied the compilation as custom attributes (using the <c>[assembly: MyAspect]</c> syntax).
    /// </summary>
    /// <remarks>
    /// <para>This class is a redundant helper class. The aspect framework only respects the <see cref="IAspect{T}"/> interface.</para>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Assembly )]
    public abstract class CompilationAspect : Aspect, IAspect<ICompilation>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<ICompilation> builder ) { }

        /// <inheritdoc />
        public virtual void BuildEligibility( IEligibilityBuilder<ICompilation> builder ) { }
    }
}