// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Options;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Represents a set of declarations and offers the ability to add aspects and set options to them. It inherits from <see cref="IValidatorReceiver{TDeclaration}"/>,
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
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Default"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible(
            Type aspectType,
            Func<TDeclaration, IAspect> createAspect,
            EligibleScenarios eligibility = EligibleScenarios.Default | EligibleScenarios.Inheritance );

        /// <summary>
        /// Adds an aspect to the current set of declarations or throws an exception if the aspect is not eligible for the aspect.
        /// </summary>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        void AddAspect<TAspect>( Func<TDeclaration, TAspect> createAspect )
            where TAspect : class, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations but only if the aspect is eligible for the declaration. 
        /// </summary>
        /// <param name="createAspect">A function that returns the aspect for a given declaration.</param>
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Default"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible<TAspect>(
            Func<TDeclaration, TAspect> createAspect,
            EligibleScenarios eligibility = EligibleScenarios.Default | EligibleScenarios.Inheritance )
            where TAspect : class, IAspect<TDeclaration>;

        /// <summary>
        /// Adds an aspect to the current set of declarations or throws an exception if the aspect is not eligible for the aspect. This overload creates a new instance of the
        /// aspect class for each target declaration.
        /// </summary>
        void AddAspect<TAspect>()
            where TAspect : class, IAspect<TDeclaration>, new();

        /// <summary>
        /// Adds an aspect to the current set of declarations using the default constructor of the aspect type. This method
        /// does not verify the eligibility of the declaration for the aspect unless you specify the <paramref name="eligibility"/> parameter.
        /// This overload creates a new instance of the aspect class for each eligible target declaration.
        /// </summary>
        /// <param name="eligibility">The scenarios for which the aspect may be eligible. The default value is <see cref="EligibleScenarios.Default"/> | <see cref="EligibleScenarios.Inheritance"/>.
        /// If <see cref="EligibleScenarios.None"/> is provided, eligibility is not checked.
        /// </param>
        void AddAspectIfEligible<TAspect>( EligibleScenarios eligibility = EligibleScenarios.Default | EligibleScenarios.Inheritance )
            where TAspect : class, IAspect<TDeclaration>, new();

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
            where TAspect : class, IAspect<TDeclaration>, new();

        /// <summary>
        /// Selects members of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
        /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>,
        /// <see cref="IValidatorReceiver.Validate"/>
        /// or <see cref="IValidatorReceiver.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds,bool)"/>.
        /// </summary>
        /// <remarks>
        /// <para>The query on the <i>right</i> part of <see cref="SelectMany{TMember}"/> is executed concurrently. It is therefore preferable to use the <see cref="Where"/>, <see cref="Select{TMember}"/>
        /// or <see cref="SelectMany{TMember}"/> methods of the current interface instead of using the equivalent system methods inside the <paramref name="selector"/> query.</para>
        /// </remarks>
        new IAspectReceiver<TMember> SelectMany<TMember>( Func<TDeclaration, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration;

        /// <summary>
        /// Selects a member or the parent of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
        /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>.  <see cref="IValidatorReceiver.Validate"/>
        /// or <see cref="IValidatorReceiver.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds,bool)"/>.
        /// </summary>
        new IAspectReceiver<TMember> Select<TMember>( Func<TDeclaration, TMember> selector )
            where TMember : class, IDeclaration;

        /// <summary>
        /// Selects all types in the current context. If the current object represents <see cref="ICompilation"/> or <see cref="INamespace"/>, this
        /// method returns all the types in the compilation or namespace. If the current object represents a set of types, this method returns
        /// the current set. If the current object represent a set of members or parameters, the method will return their declaring types.
        /// </summary>
        /// <param name="includeNestedTypes">Indicates whether nested types should be recursively included in the output.</param>
        /// <remarks>
        /// <para>The query on the <i>right</i> part of <see cref="SelectTypes"/> is executed concurrently.</para>. 
        /// </remarks>
        new IAspectReceiver<INamedType> SelectTypes( bool includeNestedTypes = false );

        /// <summary>
        /// Filters the set of declarations included in the current set.
        /// </summary>
        new IAspectReceiver<TDeclaration> Where( Func<TDeclaration, bool> predicate );

        /// <summary>
        /// Sets options for the declarations in the current set of declarations by supplying a <see cref="Func{TResult}"/>.
        /// </summary>
        /// <param name="func">A function giving the options for the given declaration.</param>
        /// <typeparam name="TOptions">The type of options.</typeparam>
        /// <remarks>
        /// This method should only set the option properties that need to be changed. All unchanged properties must be let null.
        /// </remarks>
        void SetOptions<TOptions>( Func<TDeclaration, TOptions> func )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<TDeclaration>, new();

        /// <summary>
        /// Sets options for the declarations in the current set of declarations by supplying a <see cref="Func{TResult}"/>.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <typeparam name="TOptions">The type of options.</typeparam>
        /// <remarks>
        /// This method should only set the option properties that need to be changed. All unchanged properties must be let null.
        /// </remarks>
        void SetOptions<TOptions>( TOptions options )
            where TOptions : class, IHierarchicalOptions, IHierarchicalOptions<TDeclaration>, new();
    }
}