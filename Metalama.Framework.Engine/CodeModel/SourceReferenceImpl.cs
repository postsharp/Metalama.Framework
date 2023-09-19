// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class SourceReferenceImpl : ISourceReferenceImpl
{
    public static SourceReferenceImpl Instance { get; } = new();

    private SourceReferenceImpl() { }

    IDiagnosticLocation ISourceReferenceImpl.GetDiagnosticLocation( in SourceReference sourceReference )
        => sourceReference.NodeOrToken switch
        {
            SyntaxNode node => new LocationWrapper( node.GetDiagnosticLocation() ),
            SyntaxToken token => new LocationWrapper( token.GetLocation() ),
            _ => throw new AssertionFailedException( $"Unexpected type {sourceReference.NodeOrToken.GetType()}." )
        };

    string ISourceReferenceImpl.GetKind( in SourceReference sourceReference )
        => sourceReference.NodeOrToken switch
        {
            SyntaxNode node => node.Kind().ToString(),
            SyntaxToken token => token.Kind().ToString(),
            _ => throw new AssertionFailedException( $"{sourceReference.NodeOrToken} is not supported" )
        };

    public SourceSpan GetSourceSpan( in SourceReference sourceReference )
    {
        var (syntaxTree, span) = sourceReference.NodeOrToken switch
        {
            SyntaxNode node => (node.SyntaxTree, node.Span),
            SyntaxToken token => (token.SyntaxTree.AssertNotNull(), token.Span),
            _ => throw new AssertionFailedException( $"{sourceReference.NodeOrToken} is not supported" )
        };

        var lineSpan = syntaxTree.GetLineSpan( span );

        return new SourceSpan(
            syntaxTree.FilePath,
            syntaxTree,
            span.Start,
            span.End,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character,
            this );
    }

    public string GetText( in SourceSpan sourceSpan )
        => ((SyntaxTree) sourceSpan.SyntaxTree).GetText().GetSubText( TextSpan.FromBounds( sourceSpan.Start, sourceSpan.End ) ).ToString();

    public string GetText( in SourceReference sourceReference, bool normalized )
        => sourceReference.NodeOrToken switch
        {
            SyntaxNode node when normalized => node.NormalizeWhitespace().ToString(),
            SyntaxNode node when !normalized => node.ToFullString(),
            SyntaxToken token when normalized => token.NormalizeWhitespace().ToString(),
            SyntaxNode token when !normalized => token.ToFullString(),
            _ => throw new AssertionFailedException( $"{sourceReference.NodeOrToken} is not supported" )
        };

    public bool IsImplementationPart( in SourceReference sourceReference )
        => !(sourceReference.NodeOrToken is MethodDeclarationSyntax { Body: null, ExpressionBody: null } method &&
             method.Modifiers.Any( m => m.IsKind( SyntaxKind.PartialKeyword ) ));
}