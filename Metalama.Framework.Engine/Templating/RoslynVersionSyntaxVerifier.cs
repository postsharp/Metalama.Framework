// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Templating;

internal partial class RoslynVersionSyntaxVerifier : CSharpSyntaxWalker
{
    private readonly IDiagnosticAdder _diagnostics;

    public RoslynApiVersion MaximalAcceptableApiVersion { get; }

    public string MaximalAcceptableLanguageVersion { get; }

    public RoslynApiVersion MaximalUsedVersion { get; private set; } = RoslynApiVersion.Lowest;

    public RoslynVersionSyntaxVerifier( IDiagnosticAdder diagnostics, RoslynApiVersion maximalAcceptableApiVersion, string maximalAcceptableLanguageVersion )
    {
        this._diagnostics = diagnostics;
        this.MaximalAcceptableApiVersion = maximalAcceptableApiVersion;
        this.MaximalAcceptableLanguageVersion = maximalAcceptableLanguageVersion;
    }

    private void OnForbiddenSyntaxUsed( in SyntaxNodeOrToken node )
    {
        this._diagnostics.Report(
            TemplatingDiagnosticDescriptors.TemplateUsesUnsupportedLanguageFeature.CreateRoslynDiagnostic(
                node.GetLocation(),
                this.MaximalAcceptableLanguageVersion ) );
    }

    // ReSharper disable once UnusedMember.Local
    private void VisitVersionSpecificNode( SyntaxNode node, RoslynApiVersion version )
    {
        if ( version > this.MaximalAcceptableApiVersion )
        {
            this.OnForbiddenSyntaxUsed( node );
        }

        if ( version > this.MaximalUsedVersion )
        {
            this.MaximalUsedVersion = version;
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void VisitVersionSpecificField( in SyntaxNodeOrToken nodeOrToken, RoslynApiVersion version )
    {
        if ( !nodeOrToken.IsKind( SyntaxKind.None ) )
        {
            if ( version > this.MaximalAcceptableApiVersion )
            {
                this.OnForbiddenSyntaxUsed( nodeOrToken );
            }

            if ( version > this.MaximalUsedVersion )
            {
                this.MaximalUsedVersion = version;
            }
        }
    }
    
    private void VisitVersionSpecificFieldKind( in SyntaxNodeOrToken nodeOrToken, RoslynApiVersion version )
    {
        if ( version > this.MaximalAcceptableApiVersion )
        {
            this.OnForbiddenSyntaxUsed( nodeOrToken );
        }

        if ( version > this.MaximalUsedVersion )
        {
            this.MaximalUsedVersion = version;
        }
    }
}