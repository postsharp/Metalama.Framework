// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Templating;

internal sealed partial class RoslynVersionSyntaxVerifier : SafeSyntaxWalker
{
    private readonly IDiagnosticAdder _diagnostics;

    [UsedImplicitly]
    public RoslynApiVersion MaximalAcceptableApiVersion { get; }

    [UsedImplicitly]
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
            TemplatingDiagnosticDescriptors.TemplateUsesUnsupportedLanguageVersion.CreateRoslynDiagnostic(
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

    // ReSharper disable once UnusedMember.Local
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