// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

public abstract class ValidatorInstance : ISyntaxReferenceImpl, IDiagnosticSource
{
    public ValidatorDriver Driver { get; }

    public IDeclaration ValidatedDeclaration { get; }

    public ValidatorImplementation Implementation { get; }

    protected ValidatorInstance( IDeclaration validatedDeclaration, ValidatorDriver driver, in ValidatorImplementation implementation, string description )
    {
        this.Driver = driver;
        this.Implementation = implementation;
        this.ValidatedDeclaration = validatedDeclaration;
        this.DiagnosticSourceDescription = description;
    }

    // TODO: ISyntaxReferenceImpl should not be implemented in this class.
    IDiagnosticLocation ISyntaxReferenceImpl.GetDiagnosticLocation( in SyntaxReference syntaxReference )
        => syntaxReference.NodeOrToken switch
        {
            SyntaxNode node => new LocationWrapper( node.GetDiagnosticLocation() ),
            SyntaxToken token => new LocationWrapper( token.GetLocation() ),
            _ => throw new AssertionFailedException( $"Unexpected type {syntaxReference.NodeOrToken.GetType()}." )
        };

    string ISyntaxReferenceImpl.GetKind( in SyntaxReference syntaxReference )
        => syntaxReference.NodeOrToken switch
        {
            SyntaxNode node => node.Kind().ToString(),
            SyntaxToken token => token.Kind().ToString(),
            _ => throw new AssertionFailedException( $"{syntaxReference.NodeOrToken} is not supported" )
        };

    public string DiagnosticSourceDescription { get; }

    public override string ToString() => $"{this.GetType().Name}: {this.DiagnosticSourceDescription}";
}