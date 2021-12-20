// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorInstance : ValidatorInstance
{
    public ReferenceValidatorInstance(
        IDeclaration validatedDeclaration,
        ValidatorDriver driver,
        in ValidatorImplementation implementation,
        ReferenceKinds referenceKinds ) : base( validatedDeclaration, driver, implementation )
    {
        this.ReferenceKinds = referenceKinds;
    }

    // Aspect or fabric.

    public ReferenceKinds ReferenceKinds { get; }

    public void Validate( IDeclaration referencingDeclaration, SyntaxNode node, ReferenceKinds referenceKind, IDiagnosticSink diagnosticAdder )
    {
        var context = new ReferenceValidationContext(
            this.ValidatedDeclaration,
            referencingDeclaration,
            new SyntaxReference( node, this ),
            this.Implementation.State,
            diagnosticAdder,
            referenceKind );

        ((ReferenceValidatorDriver) this.Driver).Validate( this.Implementation, context );
    }
}