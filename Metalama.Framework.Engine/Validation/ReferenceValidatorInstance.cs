// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

internal readonly struct ValidatorImplementation
{
    public object Implementation { get; }

    public IAspectState? State { get; }

    public static ValidatorImplementation Create( IAspectPredecessor predecessor )
        => predecessor switch
        {
            IAspectInstance aspectInstance => new ValidatorImplementation( aspectInstance ),
            IFabricInstance fabricInstance => new ValidatorImplementation( fabricInstance.Fabric ),
            _ => throw new AssertionFailedException()
        };

    public static ValidatorImplementation Create( object implementation, IAspectState? aspectState ) => new( implementation, aspectState );

    private ValidatorImplementation( IAspectInstance aspectInstance )
    {
        this.Implementation = aspectInstance.Aspect;
        this.State = aspectInstance.State;
    }

    private ValidatorImplementation( Fabric fabric )
    {
        this.Implementation = fabric;
        this.State = null;
    }

    private ValidatorImplementation( object implementation, IAspectState? aspectState )
    {
        this.Implementation = implementation;
        this.State = aspectState;
    }
}

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