// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.SyntaxGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Templating.Statements;

internal class SwitchStatement : IStatementImpl
{
    private readonly IExpression _expression;
    private readonly ImmutableArray<SwitchSection> _sections;

    public SwitchStatement( IExpression expression, ImmutableArray<SwitchSection> sections )
    {
        this._expression = expression;
        this._sections = sections;
    }

    public StatementSyntax GetSyntax( TemplateSyntaxFactoryImpl? templateSyntaxFactory )
    {
        if ( templateSyntaxFactory == null )
        {
            throw new InvalidOperationException( $"{nameof(SwitchStatementBuilder)} is not available in the current context." );
        }

        var sections = new List<SwitchSectionSyntax>();

        foreach ( var switchCase in this._sections )
        {
            SwitchLabelSyntax label;

            if ( switchCase.Label != null )
            {
                label = SyntaxFactory.CaseSwitchLabel(
                    ((IUserExpression) switchCase.Label!).ToTypedExpressionSyntax( templateSyntaxFactory.SyntaxSerializationContext ) );
            }
            else
            {
                label = SyntaxFactory.DefaultSwitchLabel();
            }

            var statement = SyntaxFactory.SingletonList( ((IStatementImpl) switchCase.Statement).GetSyntax( templateSyntaxFactory ) );

            if ( switchCase.EndWithBreak )
            {
                statement = statement.Add( SyntaxFactory.BreakStatement() );
            }

            var section = SyntaxFactory.SwitchSection( SyntaxFactory.SingletonList( label ), statement );

            sections.Add( section );
        }

        return SyntaxFactory.SwitchStatement(
            ((IUserExpression) this._expression).ToTypedExpressionSyntax( templateSyntaxFactory.SyntaxSerializationContext ).Syntax,
            SyntaxFactory.List( sections ) );
    }
}