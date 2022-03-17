// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

public class ReferenceValidatorInstance : ValidatorInstance
{
    public ReferenceValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver driver,
        ValidatorImplementation implementation,
        ReferenceKinds referenceKinds ) : base( validatedDeclaration, driver, implementation )
    {
        this.ReferenceKinds = referenceKinds;
    }

    // Aspect or fabric.

    public ReferenceKinds ReferenceKinds { get; }

    internal void Validate(
        IDeclaration referencingDeclaration,
        SyntaxNode node,
        ReferenceKinds referenceKind,
        IDiagnosticSink diagnosticAdder,
        UserCodeInvoker userCodeInvoker,
        UserCodeExecutionContext? userCodeExecutionContext )
    {
        var validationContext = new ReferenceValidationContext(
            this.ValidatedDeclaration,
            referencingDeclaration,
            new SyntaxReference( node, this ),
            this.Implementation.State,
            diagnosticAdder,
            referenceKind );

        ((ValidatorDriver<ReferenceValidationContext>) this.Driver).Validate(
            this.Implementation,
            validationContext,
            userCodeInvoker,
            userCodeExecutionContext );
    }
}