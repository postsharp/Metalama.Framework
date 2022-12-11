﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Templating;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverridePropertyOrIndexerTransformation : OverrideMemberTransformation
{
    public new IPropertyOrIndexer OverriddenDeclaration => (IPropertyOrIndexer) base.OverriddenDeclaration;

    public OverridePropertyOrIndexerTransformation(
        Advice advice,
        IPropertyOrIndexer overriddenDeclaration,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags ) { }

    /// <summary>
    /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
    /// </summary>
    protected BlockSyntax CreateIdentityAccessorBody( in MemberInjectionContext context, SyntaxKind accessorDeclarationKind )
    {
        switch ( accessorDeclarationKind )
        {
            case SyntaxKind.GetAccessorDeclaration:
                return SyntaxFactoryEx.FormattedBlock(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( SyntaxFactory.Space ),
                        this.CreateProceedGetExpression( context ),
                        SyntaxFactory.Token( SyntaxKind.SemicolonToken ) ) );

            case SyntaxKind.SetAccessorDeclaration:
            case SyntaxKind.InitAccessorDeclaration:
                return SyntaxFactoryEx.FormattedBlock( SyntaxFactory.ExpressionStatement( this.CreateProceedSetExpression( context ) ) );

            default:
                throw new AssertionFailedException( $"Unexpected SyntaxKind: {accessorDeclarationKind}." );
        }
    }

    protected abstract ExpressionSyntax CreateProceedGetExpression( in MemberInjectionContext context );

    protected abstract ExpressionSyntax CreateProceedSetExpression( in MemberInjectionContext context );
}