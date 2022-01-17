// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Validation;
using System;
using System.Linq.Expressions;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Represents a set of declarations and offers the ability to add aspects, annotations or validators to them.
    /// </summary>
    /// <typeparam name="TDeclaration"></typeparam>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IDeclarationSelection<out TDeclaration>
        where TDeclaration : class, IDeclaration
    {
        /// <summary>
        /// Registers a method that will be invoked to validate references to any declaration in the current set. This method
        /// must have a parameter of type <c>in</c> <see cref="ReferenceValidationContext"/>. Only source code references
        /// are validated. References added by aspects are ignored by design.
        /// </summary>
        /// <param name="validateMethod"></param>
        /// <param name="referenceKinds">Kinds of references that this method is interested to analyze.</param>
        void RegisterReferenceValidator( ValidatorDelegate<ReferenceValidationContext> validateMethod, ReferenceKinds referenceKinds );

        /// <summary>
        /// Registers a method that will be invoked to validate the final state (i.e. the state including the transformation by all aspects) of any declaration
        /// in the current set. This method must have a parameter of type <c>in</c> <see cref="DeclarationValidationContext"/>.  
        /// </summary>
        /// <param name="validateMethod"></param>
        void RegisterFinalValidator( ValidatorDelegate<DeclarationValidationContext> validateMethod );

        /// <summary>
        /// Adds an aspect to the current set of declarations. This overload allows adding inherited aspects.
        /// </summary>
        IDeclarationSelection<TDeclaration> AddAspect<TAspect>( Func<TDeclaration, Expression<Func<TAspect>>> createAspect )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations. This overload does not allow adding inherited aspects.
        /// </summary>
        IDeclarationSelection<TDeclaration> AddAspect<TAspect>( Func<TDeclaration, TAspect> createAspect )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations using the default constructor of the aspect type.
        /// </summary>
        IDeclarationSelection<TDeclaration> AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<TDeclaration>, new();

        /// <summary>
        /// Requires an instance of a specified aspect type to be present on a specified declaration. If the aspect
        /// is not present, this method adds a new instance of the aspect (if any) by using the default aspect constructor.
        /// </summary>
        /// <remarks>
        /// <para>Calling this method causes the current aspect to be present in the <see cref="IAspectInstance.Predecessors"/> list
        /// even if the required aspect was already present on the target declaration.</para>
        /// </remarks>
        /// <param name="target">The target declaration. It must be contained in the current type.</param>
        /// <typeparam name="TTarget">Type of the target declaration.</typeparam>
        /// <typeparam name="TAspect">Type of the aspect. The type must be ordered after the aspect type calling this method.</typeparam>
        [Obsolete( "Not implemented." )]
        IDeclarationSelection<TDeclaration> RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new();

        /// <summary>
        /// Adds an annotation to the current set of declarations.
        /// </summary>
        /// <param name="getAnnotation">A delegate that returns the annotation (a single annotation instance used several times).</param>
        /// <typeparam name="TAspect">The type of the aspect for which the annotation is meant.</typeparam>
        /// <typeparam name="TAnnotation">The type of the annotation.</typeparam>
        [Obsolete( "Not implemented." )]
        IDeclarationSelection<TDeclaration> AddAnnotation<TAspect, TAnnotation>( Func<TDeclaration, TAnnotation> getAnnotation )
            where TAspect : IAspect
            where TAnnotation : IAnnotation<TDeclaration, TAspect>, IEligible<TDeclaration>;
    }
}