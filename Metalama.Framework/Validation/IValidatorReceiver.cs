﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation;

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
    /// Registers a method that will be invoked to validate references to any declaration in the current set. This method
    /// must have a parameter of type <c>in</c> <see cref="ReferenceValidationContext"/>. Only source code references
    /// are validated. References added by aspects are ignored by design.
    /// </summary>
    /// <param name="validateMethod"></param>
    /// <param name="referenceKinds">Kinds of references that this method is interested to analyze.</param>
    void ValidateReferences( ValidatorDelegate<ReferenceValidationContext> validateMethod, ReferenceKinds referenceKinds );

    void ValidateReferences( ReferenceValidator validator );
}

[InternalImplement]
[CompileTime]
public interface IValidatorReceiver<out TDeclaration> : IValidatorReceiver
    where TDeclaration : class, IDeclaration
{
    void ValidateReferences<TValidator>( Func<TDeclaration, TValidator> validator )
        where TValidator : ReferenceValidator;

    void ReportDiagnostic( Func<TDeclaration, IDiagnostic> diagnostic );

    void SuppressDiagnostic( Func<TDeclaration, SuppressionDefinition> suppression );

    void SuggestCodeFix( Func<TDeclaration, CodeFix> codeFix );

    /// <summary>
    /// Gets an interface that allows to validate the final compilation, after all aspects have been applied.
    /// </summary>
    IValidatorReceiver<IDeclaration> AfterAllAspects();

    /// <summary>
    /// Gets an interface that allows to validate the initial compilation, after before any aspect has been applied.
    /// </summary>
    IValidatorReceiver<IDeclaration> BeforeAnyAspect();

    /// <summary>
    /// Selects members of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>,
    /// <see cref="IValidatorReceiver.Validate"/>
    /// or <see cref="IValidatorReceiver.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds)"/>.
    /// </summary>
    IValidatorReceiver<TMember> SelectMany<TMember>( Func<TDeclaration, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration;

    /// <summary>
    /// Selects a member or the parent of the target declaration of the current aspect or fabric with the purpose of adding aspects, annotations or validators to them
    /// using e.g. <see cref="IAspectReceiver{TDeclaration}.AddAspectIfEligible{TAspect}(Metalama.Framework.Eligibility.EligibleScenarios)"/>.  <see cref="IValidatorReceiver.Validate"/>
    /// or <see cref="IValidatorReceiver.ValidateReferences(Metalama.Framework.Validation.ValidatorDelegate{Metalama.Framework.Validation.ReferenceValidationContext},Metalama.Framework.Validation.ReferenceKinds)"/>.
    /// </summary>
    IValidatorReceiver<TMember> Select<TMember>( Func<TDeclaration, TMember> selector )
        where TMember : class, IDeclaration;

    IValidatorReceiver<TDeclaration> Where( Func<TDeclaration, bool> predicate );
}