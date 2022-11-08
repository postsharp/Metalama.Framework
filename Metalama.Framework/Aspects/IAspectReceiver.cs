// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Represents a set of declarations and offers the ability to add aspects, annotations to them. It inherits from <see cref="IValidatorReceiver{TDeclaration}"/>,
    /// which allows to add validators.
    /// </summary>
    [InternalImplement]
    [CompileTime]
    public interface IAspectReceiver<out TDeclaration> : IValidatorReceiver<TDeclaration>
        where TDeclaration : class, IDeclaration
    {
        /// <summary>
        /// Adds a aspect to the current set of declarations or throws an exception if the aspect is not eligible for the aspect. This overload is non-generic.
        /// </summary>
        /// <param name="aspectType">The exact type of the aspect returned by <paramref name="createAspect"/>. It is not allowed to specify a base type in this parameter, only the exact type.</param>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        void AddAspect( Type aspectType, Func<TDeclaration, IAspect> createAspect );

        /// <summary>
        /// Adds an aspect to the current set of declarations but only if the aspect is eligible for the declaration. This overload is non-generic.
        /// </summary>
        /// <param name="aspectType">The exact type of the aspect returned by <paramref name="createAspect"/>. It is not allowed to specify a base type in this parameter, only the exact type.</param>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Aspect"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible(
            Type aspectType,
            Func<TDeclaration, IAspect> createAspect,
            EligibleScenarios eligibility = EligibleScenarios.Aspect | EligibleScenarios.Inheritance );

        /// <summary>
        /// Adds an aspect to the current set of declarations or throws an exception if the aspect is not eligible for the aspect.
        /// </summary>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        void AddAspect<TAspect>( Func<TDeclaration, TAspect> createAspect )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations but only if the aspect is eligible for the declaration. 
        /// </summary>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Aspect"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible<TAspect>(
            Func<TDeclaration, TAspect> createAspect,
            EligibleScenarios eligibility = EligibleScenarios.Aspect | EligibleScenarios.Inheritance )
            where TAspect : Attribute, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations or throws an exception if the aspect is not eligible for the aspect. This overload creates a new instance of the
        /// aspect class for each target declaration.
        /// </summary>
        void AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<TDeclaration>, new();

        /// <summary>
        /// Adds an aspect to the current set of declarations using the default constructor of the aspect type. This method
        /// does not verify the eligibility of the declaration for the aspect unless you specify the <paramref name="eligibility"/> parameter.
        /// This overload creates a new instance of the aspect class for each eligible target declaration.
        /// </summary>
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Aspect"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible<TAspect>( EligibleScenarios eligibility = EligibleScenarios.Aspect | EligibleScenarios.Inheritance )
            where TAspect : Attribute, IAspect<TDeclaration>, new();

        /// <summary>
        /// Requires an instance of a specified aspect type to be present on a specified declaration. If the aspect
        /// is not present, this method adds a new instance of the aspect by using the default aspect constructor. 
        /// </summary>
        /// <remarks>
        /// <para>Calling this method causes the current aspect to be present in the <see cref="IAspectPredecessor.Predecessors"/> list
        /// even if the required aspect was already present on the target declaration.</para>
        /// </remarks>
        /// <typeparam name="TAspect">Type of the aspect. The type must be ordered after the aspect type calling this method.</typeparam>
        void RequireAspect<TAspect>()
            where TAspect : IAspect<TDeclaration>, new();

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