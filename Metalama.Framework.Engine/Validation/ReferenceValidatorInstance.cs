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
        ValidatorSource source,
        IDeclaration declaration,
        ValidatedReferenceKinds referenceKinds ) : base( source, declaration )
    {
        this.ReferenceKinds = referenceKinds;
    }

    // Aspect or fabric.

    public ValidatedReferenceKinds ReferenceKinds { get; }

    public void Validate( IDeclaration referencingDeclaration, SyntaxNode node, ValidatedReferenceKinds referenceKind, IDiagnosticSink diagnosticAdder )
    {
        var context = new ReferenceValidationContext(
            this.ValidatedDeclaration,
            referencingDeclaration,
            new SyntaxReference( node, this ),
            this.State,
            diagnosticAdder,
            referenceKind );

        ((ReferenceValidatorDriver) this.Source.Driver).Validate( this.Object, context );
    }
}