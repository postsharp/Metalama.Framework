// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.MetaModel
{

    public interface IDynamicMember
    {
        RuntimeExpression CreateExpression();
    }

    // TODO: Smell
    internal interface IDynamicMemberDifferentiated : IDynamicMember
    {
        RuntimeExpression CreateMemberAccessExpression( string member );
    }

    public interface IProceedImpl
    {
        TypeSyntax CreateTypeSyntax();

        StatementSyntax CreateAssignStatement( SyntaxToken returnValueLocalName );

        StatementSyntax CreateReturnStatement();
    }
}