// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using System;

namespace Metalama.Framework.Aspects;

[InternalImplement]
[CompileTime]
public interface IValidatorReceiver<out TDeclaration>
    where TDeclaration : class, IDeclaration
{
    /// <summary>
    /// Registers a method that will be invoked to validate the final state (i.e. the state including the transformation by all aspects) of any declaration
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

    void ReportDiagnostic( Func<TDeclaration, IDiagnostic> diagnostic );

    void SuppressDiagnostic( Func<TDeclaration, SuppressionDefinition> suppression );

    void SuggestCodeFix( Func<TDeclaration, CodeFix> codeFix );
}