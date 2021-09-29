// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Validation;
using System;
using System.Linq.Expressions;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents a set of declarations and offers the ability to add aspects and annotations to them.
    /// </summary>
    /// <typeparam name="TDeclaration"></typeparam>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IDeclarationSelection<out TDeclaration>
        where TDeclaration : class, IDeclaration
    {
        /// <summary>
        /// Adds an aspect to the current set of declarations. This overloads allows to add inherited aspects.
        /// </summary>
        void AddAspect<TAspect>( Func<TDeclaration, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations. This overloads does not allow to add inherited aspects.
        /// </summary>
        void AddAspect<TAspect>( Func<TDeclaration, TAspect> createAspect )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations using the default constructor of the aspect type.
        /// </summary>
        void AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<TDeclaration>, new();

        /// <summary>
        /// Requires an instance of a specified aspect type to be present on a specified declaration. If the aspect
        /// is not present, this method adds a new instance of the aspect (if any) by using the default aspect constructor.
        /// </summary>
        /// <remarks>
        /// <para>Calling this method causes the current aspect to be present in the <see cref="IAspectLayerBuilder.UpstreamAspects"/> list
        /// even if the required aspect was already present on the target declaration.</para>
        /// </remarks>
        /// <param name="target">The target declaration. It must be contained in the current type.</param>
        /// <typeparam name="TTarget">Type of the target declaration.</typeparam>
        /// <typeparam name="TAspect">Type of the aspect. The type must be ordered after the current aspect type.</typeparam>
        [Obsolete( "Not implemented." )]
        void RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new();

        /// <summary>
        /// Adds an annotation to the current set of declarations.
        /// </summary>
        /// <param name="getAnnotation">A delegate that returns the annotation (a single annotation instance used several times).</param>
        /// <typeparam name="TAspect">The type of the aspect for which the annotation is meant.</typeparam>
        /// <typeparam name="TAnnotation">The type of the annotation.</typeparam>
        [Obsolete( "Not implemented." )]
        void AddAnnotation<TAspect, TAnnotation>( Func<TDeclaration, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<TDeclaration, TAspect>, IEligible<TDeclaration>;
    }
}