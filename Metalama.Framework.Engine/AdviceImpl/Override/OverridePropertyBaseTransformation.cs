﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Templating.Expressions;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

internal abstract class OverridePropertyBaseTransformation : OverridePropertyOrIndexerTransformation
{
    private new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

    protected OverridePropertyBaseTransformation(
        Advice advice,
        IProperty overriddenDeclaration,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags ) { }

    protected IEnumerable<InjectedMember> GetInjectedMembersImpl(
        MemberInjectionContext context,
        BlockSyntax? getAccessorBody,
        BlockSyntax? setAccessorBody )
    {
        var propertyName = context.InjectionNameProvider.GetOverrideName(
            this.OverriddenDeclaration.DeclaringType,
            this.ParentAdvice.AspectLayerId,
            this.OverriddenDeclaration );

        var setAccessorDeclarationKind = (this.OverriddenDeclaration.IsStatic, this.OverriddenDeclaration.Writeability) switch
        {
            (true, not Writeability.None) => SyntaxKind.SetAccessorDeclaration,
            (false, Writeability.ConstructorOnly) =>
                context.SyntaxGenerationContext.SupportsInitAccessors ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration,
            (false, Writeability.InitOnly) => SyntaxKind.InitAccessorDeclaration,
            (false, Writeability.All) => SyntaxKind.SetAccessorDeclaration,
            _ => SyntaxKind.None
        };

        var modifiers = this.OverriddenDeclaration
            .GetSyntaxModifierList( ModifierCategories.Static | ModifierCategories.Unsafe )
            .Insert( 0, SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.PrivateKeyword ) );

        var overrides = new[]
        {
            new InjectedMember(
                this,
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    modifiers,
                    context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration )
                        .WithOptionalTrailingTrivia( SyntaxFactory.ElasticSpace, context.SyntaxGenerationContext.Options ),
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
                InjectedMemberSemantic.Override,
                this.OverriddenDeclaration )
        };

        return overrides;
    }

    protected SyntaxUserExpression CreateProceedDynamicExpression( MemberInjectionContext context, IMethod accessor, TemplateKind templateKind )
        => accessor.MethodKind switch
        {
            MethodKind.PropertyGet => ProceedHelper.CreateProceedDynamicExpression(
                context.SyntaxGenerationContext,
                this.CreateProceedGetExpression( context ),
                templateKind,
                this.OverriddenDeclaration.GetMethod.AssertNotNull() ),
            MethodKind.PropertySet => new SyntaxUserExpression(
                this.CreateProceedSetExpression( context ),
                this.OverriddenDeclaration.Compilation.GetCompilationModel().Cache.SystemVoidType ),
            _ => throw new AssertionFailedException( $"Unexpected MethodKind for '{accessor}': {accessor.MethodKind}." )
        };

    protected override ExpressionSyntax CreateProceedGetExpression( MemberInjectionContext context )
        => TransformationHelper.CreatePropertyProceedGetExpression(
            context.AspectReferenceSyntaxProvider,
            context.SyntaxGenerationContext,
            this.OverriddenDeclaration,
            this.ParentAdvice.AspectLayerId );

    protected override ExpressionSyntax CreateProceedSetExpression( MemberInjectionContext context )
        => TransformationHelper.CreatePropertyProceedSetExpression(
            context.AspectReferenceSyntaxProvider,
            context.SyntaxGenerationContext,
            this.OverriddenDeclaration,
            this.ParentAdvice.AspectLayerId );
}