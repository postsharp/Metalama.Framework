// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Policies
{
    [InternalImplement]
    [Obsolete( "Not implemented." )]
    public interface IProjectPolicyBuilder
    {
        // The builder intentionally does not give write access to project properties. All configuration must use IProjectExtension.

        IProject Project { get; }

        // The builder intentionally does not give access to any ICompilation because project policies are compilation-independent.

        INamedTypeSelection WithTypes( Func<ICompilation, IEnumerable<INamedType>> typeQuery );

        /// <summary>
        /// Adds a validator, which gets executed after all aspects have been added to the compilation.
        /// </summary>
        /// <param name="validator"></param>
        void AddValidator( Action<ValidateDeclarationContext<ICompilation>> validator );

        /// <summary>
        /// Register a rule that can provide annotations for a given aspect type, on demand.
        /// </summary>
        /// <param name="getAnnotation">A delegate that returns an annotation instance, or <c>null</c> if no annotation is needed.</param>
        /// <typeparam name="TTarget">The type of the target of the annotation.</typeparam>
        /// <typeparam name="TAspect">The type of the aspect.</typeparam>
        /// <typeparam name="TAnnotation">The type of the annotation.</typeparam>
        void AddAnnotationRule<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> getAnnotation )
            where TAnnotation : IAnnotation<TTarget, TAspect>
            where TAspect : IAspect
            where TTarget : class, IDeclaration;
    }
}