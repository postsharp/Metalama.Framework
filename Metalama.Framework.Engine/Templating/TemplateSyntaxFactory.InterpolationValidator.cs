// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Metalama.Framework.Engine.Templating;

public  partial class TemplateSyntaxFactory
{
    private class InterpolationValidator : SafeSyntaxVisitor<bool>
    {
        public static InterpolationValidator Instance { get; } = new();

        private InterpolationValidator() { }

        public override bool DefaultVisit( SyntaxNode node ) => node.ChildNodes().Any( this.Visit );

        public override bool VisitAliasQualifiedName( AliasQualifiedNameSyntax node ) => true;

        public override bool VisitInvocationExpression( InvocationExpressionSyntax node ) => this.Visit( node.Expression );

        public override bool VisitElementAccessExpression( ElementAccessExpressionSyntax node ) => this.Visit( node.Expression );

        public override bool VisitParenthesizedExpression( ParenthesizedExpressionSyntax node ) => false;

        public override bool VisitParenthesizedPattern( ParenthesizedPatternSyntax node ) => false;

        public override bool VisitTypeOfExpression( TypeOfExpressionSyntax node ) => false;

        public override bool VisitDefaultExpression( DefaultExpressionSyntax node ) => false;
    }
}