// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

public static class DiagnosticLocationExtensions
{
    public static IDiagnosticLocation ToDiagnosticLocation( this Location location ) => new LocationWrapper( location );

    public static IDiagnosticLocation GetDiagnosticLocation( this SyntaxNode syntaxNode ) => syntaxNode.GetLocationForDiagnostic().ToDiagnosticLocation();

    public static IDiagnosticLocation GetDiagnosticLocation( this SyntaxToken syntaxToken ) => syntaxToken.GetLocation().ToDiagnosticLocation();

    [return: NotNullIfNotNull( nameof(node) )]
    internal static Location? GetLocationForDiagnostic( this SyntaxNode? node )
        => node switch
        {
            null => null,
            MethodDeclarationSyntax method => method.Identifier.GetLocation(),
            EventDeclarationSyntax @event => @event.Identifier.GetLocation(),
            PropertyDeclarationSyntax property => property.Identifier.GetLocation(),
            IndexerDeclarationSyntax indexer => indexer.ThisKeyword.GetLocation(),
            OperatorDeclarationSyntax @operator => @operator.OperatorKeyword.GetLocation(),
            ConversionOperatorDeclarationSyntax @operator => @operator.OperatorKeyword.GetLocation(),
            BaseTypeDeclarationSyntax type => type.Identifier.GetLocation(),
            ParameterSyntax parameter => parameter.Identifier.GetLocation(),
            AccessorDeclarationSyntax accessor => accessor.Keyword.GetLocation(),
            DestructorDeclarationSyntax destructor => destructor.Identifier.GetLocation(),
            ConstructorDeclarationSyntax constructor => constructor.Identifier.GetLocation(),
            TypeParameterSyntax typeParameter => typeParameter.Identifier.GetLocation(),
            VariableDeclaratorSyntax variable => variable.Identifier.GetLocation(),
            DelegateDeclarationSyntax @delegate => @delegate.Identifier.GetLocation(),
            NameEqualsSyntax nameEquals => nameEquals.Name.GetLocation(),
            _ => node.GetLocation(),
        };
}
