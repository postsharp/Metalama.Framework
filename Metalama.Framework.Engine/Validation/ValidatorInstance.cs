// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SyntaxReference = Metalama.Framework.Code.SyntaxReference;

namespace Metalama.Framework.Engine.Validation;

internal class LocationWrapper : IDiagnosticLocationImpl
{
    public Location? DiagnosticLocation { get;}

    public LocationWrapper( Location? diagnosticLocation ) {
        this.DiagnosticLocation = diagnosticLocation;
    }
}

internal abstract class ValidatorInstance : ISyntaxReferenceService
{
    public ValidatorSource Source { get; }
    public IDeclaration ValidatedDeclaration { get; }

    public object Object => this.Source.Predecessor.Instance;

    public IAspectState? State => (this.Object as IAspectInstance)?.State;

    public ValidatorInstance( ValidatorSource source, IDeclaration validatedDeclaration )
    {
        this.Source = source;
        this.ValidatedDeclaration = validatedDeclaration;
    }

    // TODO: ISyntaxReferenceService should not be implemented in this class.
    public IDiagnosticLocation GetDiagnosticLocation( in SyntaxReference syntaxReference ) 
        => syntaxReference.NodeOrToken switch
    {
        SyntaxNode node => new LocationWrapper( node.GetLocation() ),
        SyntaxToken token => new LocationWrapper( token.GetLocation() ),
        _ => throw new AssertionFailedException()
    };

    public string GetKind( in SyntaxReference syntaxReference )
        => syntaxReference.NodeOrToken switch
        {
            SyntaxNode node => node.Kind().ToString(),
            SyntaxToken token => token.Kind().ToString(),
            _ => throw new AssertionFailedException()
        };

}