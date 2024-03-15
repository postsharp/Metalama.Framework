// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Linking.Substitution;

internal sealed class RecordParameterSubstitution : SyntaxNodeSubstitution
{
    private readonly ParameterSyntax _rootNode;
    private readonly IMethodSymbol _targetAccessor;
    private readonly string? _returnVariableIdentifier;

    public RecordParameterSubstitution(
        CompilationContext compilationContext,
        ParameterSyntax rootNode,
        IMethodSymbol targetAccessor,
        string? returnVariableIdentifier )
        : base( compilationContext )
    {
        this._rootNode = rootNode;
        this._targetAccessor = targetAccessor;
        this._returnVariableIdentifier = returnVariableIdentifier;
    }

    public override SyntaxNode TargetNode => this._rootNode;

    public override SyntaxNode Substitute( SyntaxNode currentNode, SubstitutionContext substitutionContext )
    {
        switch (currentNode, this._targetAccessor.MethodKind)
        {
            case (ParameterSyntax, MethodKind.PropertyGet):
                if ( this._returnVariableIdentifier != null )
                {
                    return
                        SyntaxFactoryEx.FormattedBlock(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName( this._returnVariableIdentifier ),
                                        CreateFieldAccessExpression() ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }
                else
                {
                    return
                        SyntaxFactoryEx.FormattedBlock(
                                ReturnStatement(
                                    Token( TriviaList(), SyntaxKind.ReturnKeyword, TriviaList( ElasticSpace ) ),
                                    CreateFieldAccessExpression(),
                                    Token( TriviaList(), SyntaxKind.SemicolonToken, TriviaList( ElasticLineFeed ) ) ) )
                            .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );
                }

            case (ParameterSyntax, MethodKind.PropertySet):
                return
                    SyntaxFactoryEx.FormattedBlock(
                            ExpressionStatement(
                                AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression,
                                    CreateFieldAccessExpression(),
                                    IdentifierName( "value" ) ) ) )
                        .WithLinkerGeneratedFlags( LinkerGeneratedFlags.FlattenableBlock );

            default:
                throw new AssertionFailedException( $"({currentNode.Kind()}, {this._targetAccessor.MethodKind}) are not supported." );
        }

        ExpressionSyntax CreateFieldAccessExpression()
        {
            return
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    IdentifierName( LinkerRewritingDriver.GetBackingFieldName( (IPropertySymbol) this._targetAccessor.AssociatedSymbol.AssertNotNull() ) ) );
        }
    }
}