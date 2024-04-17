// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation;

/// <summary>
/// The non-generic base interface for <see cref="IValidatorReceiver{TDeclaration}"/>. Represents a set of declarations to which
/// validators, diagnostics and code fix suggestions can be added.
/// </summary>
[InternalImplement]
[CompileTime]
public interface IValidatorReceiver
{
    /// <summary>
    /// Registers a method that will be invoked to validate any declaration
    /// in the current set. This method must have a parameter of type <c>in</c> <see cref="DeclarationValidationContext"/>.  
    /// </summary>
    /// <param name="validateMethod"></param>
    void Validate( ValidatorDelegate<DeclarationValidationContext> validateMethod );

    /// <summary>
    /// Registers a reference validator, provided as a delegate. The reference validator will be invoked to validate references to any declaration in the
    /// current set. This method must have a parameter of type <c>in</c> <see cref="ReferenceValidationContext"/>. Only source code references are validated.
    /// References added by aspects are ignored by design.
    /// </summary>
    /// <param name="validateMethod"></param>
    /// <param name="referenceKinds">Kinds of references that this method is interested to analyze. By default, all references are analyzed.</param>
    /// <param name="includeDerivedTypes"></param>
    void ValidateReferences(
        ValidatorDelegate<ReferenceValidationContext> validateMethod,
        ReferenceKinds referenceKinds = ReferenceKinds.All,
        bool includeDerivedTypes = false );

    /// <summary>
    /// Registers a reference validator, provided as an instance of the <see cref="ReferenceValidator"/> abstract class. The reference validator  will be
    /// invoked to validate references to any declaration in the current set. Only source code references are validated.
    /// References added by aspects are ignored by design.
    /// </summary>
    void ValidateReferences( ReferenceValidator validator );
}

/// <summary>
/// Represents a set of declarations to which
/// validators, diagnostics, code fix suggestions, and options can be added.
/// </summary>
/// <typeparam name="TDeclaration">The type of declarations in the current set.</typeparam>
[InternalImplement]
[CompileTime]
public interface IValidatorReceiver<out TDeclaration> : IValidatorReceiver
    where TDeclaration : class, IDeclaration
{
    /// <summary>
    /// Gets the current project.
    /// </summary>
    IProject Project { get; }

    /// <summary>
    /// Gets the current namespace, i.e. the one of the originating fabric or aspect instance.
    /// </summary>
    string? OriginatingNamespace { get; }

    /// <summary>
    /// Gets the declaration of the originating fabric or aspect instance.
    /// </summary>
    IRef<IDeclaration> OriginatingDeclaration { get; }

    /// <summary>
    /// Registers a reference validator, provided by a delegate that provides an instance of the <see cref="ReferenceValidator"/> abstract class.
    /// The reference validator will be invoked to validate references to any declaration in the current set. Only source code references
    /// are validated. References added by aspects are ignored by design.
    /// </summary>
    void ValidateReferences<TValidator>( Func<TDeclaration, TValidator> validator )
        where TValidator : ReferenceValidator;

    /// <summary>
    /// Reports a diagnostic for each declaration selected by the the current object.
    /// </summary>
    /// <param name="diagnostic">A function returning an <see cref="IDiagnostic"/> given a declaration.</param>
    void ReportDiagnostic( Func<TDeclaration, IDiagnostic> diagnostic );

    /// <summary>
    /// Suppresses a diagnostic for each declaration selected by the current object.
    /// </summary>
    /// <param name="suppression">A function returning a <see cref="SuppressionDefinition"/> given a declaration.</param>
    void SuppressDiagnostic( Func<TDeclaration, SuppressionDefinition> suppression );

    /// <summary>
    /// Suggests a code fix for each declaration selected by the current object.
    /// </summary>
    /// <param name="codeFix">A function returning a <see cref="CodeFix"/> given a declaration.</param>
    void SuggestCodeFix( Func<TDeclaration, CodeFix> codeFix );

    /// <summary>
    /// Gets an interface that allows to validate the final compilation, after all aspects have been applied.
    /// </summary>
    IValidatorReceiver<TDeclaration> AfterAllAspects();

    /// <summary>
    /// Gets an interface that allows to validate the initial compilation, after before any aspect has been applied.
    /// </summary>
    IValidatorReceiver<TDeclaration> BeforeAnyAspect();

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
    IValidatorReceiver<TMember> SelectMany<TMember>( Func<TDeclaration, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects a single member or the parent of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>.  <see cref="IValidatorReceiver.Validate"/>
    /// or <see cref="IValidatorReceiver.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds,bool)"/>.
    /// </summary>
    IValidatorReceiver<TMember> Select<TMember>( Func<TDeclaration, TMember> selector )
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
    IValidatorReceiver<INamedType> SelectTypes( bool includeNestedTypes = false );

    /// <summary>
    /// Filters the set of declarations included in the query.
    /// </summary>
    IValidatorReceiver<TDeclaration> Where( Func<TDeclaration, bool> predicate );
}