﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverridePropertyBaseTransformation : OverridePropertyOrIndexerTransformation
{
    public new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

    public OverridePropertyBaseTransformation(
        Advice advice,
        IProperty overriddenDeclaration,
        IObjectReader tags )
        : base( advice, overriddenDeclaration, tags ) { }

    protected IEnumerable<InjectedMember> GetInjectedMembersImpl(
        in MemberInjectionContext context,
        BlockSyntax? getAccessorBody,
        BlockSyntax? setAccessorBody )
    {
        var propertyName = context.InjectionNameProvider.GetOverrideName(
            this.OverriddenDeclaration.DeclaringType,
            this.ParentAdvice.AspectLayerId,
            this.OverriddenDeclaration );

        var setAccessorDeclarationKind =
            this.OverriddenDeclaration.Writeability is Writeability.InitOnly or Writeability.ConstructorOnly
                ? SyntaxKind.InitAccessorDeclaration
                : SyntaxKind.SetAccessorDeclaration;

        var modifiers = this.OverriddenDeclaration
            .GetSyntaxModifierList( ModifierCategories.Static )
            .Insert( 0, SyntaxFactory.Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( SyntaxFactory.Space ) );

        var overrides = new[]
        {
            new InjectedMember(
                this,
                SyntaxFactory.PropertyDeclaration(
                    SyntaxFactory.List<AttributeListSyntax>(),
                    modifiers,
                    context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration ).WithTrailingTrivia( SyntaxFactory.Space ),
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

    protected BuiltUserExpression CreateProceedDynamicExpression( in MemberInjectionContext context, IMethod accessor, TemplateKind templateKind )
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
            _ => throw new AssertionFailedException( $"Unexpected MethodKind for '{accessor}': {accessor.MethodKind}." )
        };

    protected override ExpressionSyntax CreateProceedGetExpression( in MemberInjectionContext context )
        => context.AspectReferenceSyntaxProvider.GetPropertyReference(
            this.ParentAdvice.AspectLayerId,
            this.OverriddenDeclaration,
            AspectReferenceTargetKind.PropertyGetAccessor,
            context.SyntaxGenerator );

    protected override ExpressionSyntax CreateProceedSetExpression( in MemberInjectionContext context )
        => SyntaxFactory.AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            context.AspectReferenceSyntaxProvider.GetPropertyReference(
                this.ParentAdvice.AspectLayerId,
                this.OverriddenDeclaration,
                AspectReferenceTargetKind.PropertySetAccessor,
                context.SyntaxGenerator ),
            SyntaxFactory.IdentifierName( "value" ) );
}