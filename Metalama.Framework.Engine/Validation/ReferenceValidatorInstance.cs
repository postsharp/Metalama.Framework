// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceValidatorInstance : ValidatorInstance, IReferenceValidatorProperties
{
    public ReferenceValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver driver,
        ValidatorImplementation implementation,
        ReferenceKinds referenceKinds,
        bool includeDerivedTypes,
        string description ) : base( validatedDeclaration, driver, implementation, description )
    {
        this.ReferenceKinds = referenceKinds;
        this.IncludeDerivedTypes = includeDerivedTypes;
    }

    // Aspect or fabric.

    public ReferenceKinds ReferenceKinds { get; }

    public bool IncludeDerivedTypes { get; }

    public DeclarationKind ValidatedDeclarationKind => this.ValidatedDeclaration.DeclarationKind;

    internal void Validate(
        IDeclaration referencingDeclaration,
        in SyntaxNodeOrToken node,
        ReferenceKinds referenceKind,
        IDiagnosticSink diagnosticAdder,
        UserCodeInvoker userCodeInvoker,
        UserCodeExecutionContext userCodeExecutionContext )
    {
        var validationContext = new ReferenceValidationContext(
            this.ValidatedDeclaration,
            referencingDeclaration,
            new SyntaxReference( node.AsNode() ?? (object) node.AsToken(), this ),
            this.Implementation.State,
            diagnosticAdder,
            this,
            referenceKind );

        ((ValidatorDriver<ReferenceValidationContext>) this.Driver).Validate(
            this.Implementation,
            validationContext,
            userCodeInvoker,
            userCodeExecutionContext );
    }
}