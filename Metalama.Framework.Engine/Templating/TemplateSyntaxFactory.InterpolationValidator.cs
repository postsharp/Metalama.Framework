// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Templating;

internal class InterpolationSyntaxHelper : SafeSyntaxVisitor<bool>
{
    private static readonly InterpolationSyntaxHelper _instance = new();

    private InterpolationSyntaxHelper() { }

    public override bool DefaultVisit( SyntaxNode node ) => node.ChildNodes().Any( this.Visit );

    public override bool VisitAliasQualifiedName( AliasQualifiedNameSyntax node ) => true;

    public override bool VisitInvocationExpression( InvocationExpressionSyntax node ) => this.Visit( node.Expression );

    public override bool VisitElementAccessExpression( ElementAccessExpressionSyntax node ) => this.Visit( node.Expression );

    public override bool VisitParenthesizedExpression( ParenthesizedExpressionSyntax node ) => false;

    public override bool VisitParenthesizedPattern( ParenthesizedPatternSyntax node ) => false;

    public override bool VisitTypeOfExpression( TypeOfExpressionSyntax node ) => false;

    public override bool VisitDefaultExpression( DefaultExpressionSyntax node ) => false;

    public static InterpolationSyntax Fix( InterpolationSyntax interpolation )
    {
        // If the interpolation expression contains an alias-prefixed identifier (for instance global::System) that is not
        // in a parenthesis or a square bracket, we need to parenthesize the expression.

        if ( _instance.Visit( interpolation ) )
        {
            return interpolation.WithExpression( SyntaxFactory.ParenthesizedExpression( interpolation.Expression ) );
        }
        else
        {
            return interpolation;
        }
    }
}