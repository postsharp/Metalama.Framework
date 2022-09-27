// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverridePropertyBaseTransformation : OverrideMemberTransformation
{
    public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

    public OverridePropertyBaseTransformation(
        Advice advice,
        IProperty overriddenDeclaration,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags ) { }

    protected IEnumerable<IntroducedMember> GetIntroducedMembersImpl(
        in MemberIntroductionContext context,
        BlockSyntax? getAccessorBody,
        BlockSyntax? setAccessorBody )
    {
        var propertyName = context.IntroductionNameProvider.GetOverrideName(
            this.OverriddenDeclaration.DeclaringType,
            this.ParentAdvice.AspectLayerId,
            this.OverriddenDeclaration );

        var setAccessorDeclarationKind = this.OverriddenDeclaration.Writeability == Writeability.InitOnly
            ? SyntaxKind.InitAccessorDeclaration
            : SyntaxKind.SetAccessorDeclaration;

        var modifiers = this.OverriddenDeclaration
            .GetSyntaxModifierList( ModifierCategories.Static )
            .Insert( 0, SyntaxFactory.Token( SyntaxKind.PrivateKeyword ) );

        var overrides = new[]
        {
            new IntroducedMember(
                this,
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    modifiers,
                    context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration ),
                    null,
                    SyntaxFactory.Identifier( propertyName ),
                    SyntaxFactory.AccessorList(
                        SyntaxFactory.List(
                            new[]
                                {
                                    getAccessorBody != null
                                        ? SyntaxFactory.AccessorDeclaration(
                                            SyntaxKind.GetAccessorDeclaration,
                                            SyntaxFactory.List<AttributeListSyntax>(),
                                            default,
                                            getAccessorBody )
                                        : null,
                                    setAccessorBody != null
                                        ? SyntaxFactory.AccessorDeclaration(
                                            setAccessorDeclarationKind,
                                            SyntaxFactory.List<AttributeListSyntax>(),
                                            default,
                                            setAccessorBody )
                                        : null
                                }.Where( a => a != null )
                                .AssertNoneNull() ) ),
                    null,
                    null ),
                this.ParentAdvice.AspectLayerId,
                IntroducedMemberSemantic.Override,
                this.OverriddenDeclaration )
        };

        return overrides;
    }

    protected BuiltUserExpression CreateProceedDynamicExpression( in MemberIntroductionContext context, IMethod accessor, TemplateKind templateKind )
        => accessor.MethodKind switch
        {
            MethodKind.PropertyGet => ProceedHelper.CreateProceedDynamicExpression(
                context.SyntaxGenerationContext,
                this.CreateProceedGetExpression( context ),
                templateKind,
                this.OverriddenDeclaration.GetMethod.AssertNotNull() ),
            MethodKind.PropertySet => new BuiltUserExpression(
                this.CreateProceedSetExpression( context ),
                this.OverriddenDeclaration.Compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.Void ) ),
            _ => throw new AssertionFailedException()
        };

    /// <summary>
    /// Creates a trivial passthrough body for cases where we have template only for one accessor kind.
    /// </summary>
    protected BlockSyntax? CreateIdentityAccessorBody( in MemberIntroductionContext context, SyntaxKind accessorDeclarationKind )
    {
        switch ( accessorDeclarationKind )
        {
            case SyntaxKind.GetAccessorDeclaration:
                return SyntaxFactory.Block( SyntaxFactory.ReturnStatement( this.CreateProceedGetExpression( context ) ) );

            case SyntaxKind.SetAccessorDeclaration:
            case SyntaxKind.InitAccessorDeclaration:
                return SyntaxFactory.Block( SyntaxFactory.ExpressionStatement( this.CreateProceedSetExpression( context ) ) );

            default:
                throw new AssertionFailedException();
        }
    }

    private ExpressionSyntax CreateProceedGetExpression( in MemberIntroductionContext context )
        => context.AspectReferenceSyntaxProvider.GetPropertyReference( this.ParentAdvice.AspectLayerId, this.OverriddenDeclaration, context.SyntaxGenerator );

    private ExpressionSyntax CreateProceedSetExpression( in MemberIntroductionContext context )
        => SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            context.AspectReferenceSyntaxProvider.GetPropertyReference( this.ParentAdvice.AspectLayerId, this.OverriddenDeclaration, context.SyntaxGenerator ),
            SyntaxFactory.IdentifierName( "value" ) );
}