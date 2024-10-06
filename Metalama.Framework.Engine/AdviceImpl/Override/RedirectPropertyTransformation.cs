// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.AdviceImpl.Override;

/// <summary>
/// Represents a property override, which redirects to accessors of another property without requiring template expansion.
/// </summary>
internal sealed class RedirectPropertyTransformation : OverrideMemberTransformation
{
    private readonly IRef<IProperty> _targetProperty;

    private new IRef<IProperty> OverriddenDeclaration => (IRef<IProperty>) base.OverriddenDeclaration;

    public RedirectPropertyTransformation( Advice advice, IRef<IProperty> overriddenDeclaration, IRef<IProperty> targetProperty )
        : base( advice, overriddenDeclaration, ObjectReader.Empty )
    {
        this._targetProperty = targetProperty;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        var overriddenDeclaration = this.OverriddenDeclaration.GetTarget(context.Compilation);

        return
        [
            new InjectedMember(
                this,
                PropertyDeclaration(
                    List<AttributeListSyntax>(),
                    overriddenDeclaration.GetSyntaxModifierList(),
                    context.SyntaxGenerator.PropertyType( overriddenDeclaration )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            overriddenDeclaration.DeclaringType,
                            this.AspectLayerId,
                            overriddenDeclaration ) ),
                    AccessorList( List( GetAccessors() ) ),
                    null,
                    null ),
                this.AspectLayerId,
                InjectedMemberSemantic.Override,
                overriddenDeclaration.ToRef() )
        ];

        IEnumerable<AccessorDeclarationSyntax> GetAccessors()
        {
            return new[]
                {
                    overriddenDeclaration.GetMethod != null
                        ? AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            overriddenDeclaration.GetMethod.GetSyntaxModifierList(),
                            CreateGetterBody(),
                            null )
                        : null,
                    overriddenDeclaration.SetMethod != null
                        ? AccessorDeclaration(
                            overriddenDeclaration.Writeability != Writeability.InitOnly
                                ? SyntaxKind.SetAccessorDeclaration
                                : SyntaxKind.InitAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            overriddenDeclaration.SetMethod.GetSyntaxModifierList(),
                            CreateSetterBody(),
                            null )
                        : null
                }.Where( a => a != null )
                .AssertNoneNull();
        }

        BlockSyntax CreateGetterBody()
        {
            return
                context.SyntaxGenerator.FormattedBlock(
                    ReturnStatement(
                        SyntaxFactoryEx.TokenWithTrailingSpace( SyntaxKind.ReturnKeyword ),
                        CreateAccessTargetExpression(),
                        Token( SyntaxKind.SemicolonToken ) ) );
        }

        BlockSyntax CreateSetterBody()
        {
            return
                context.SyntaxGenerator.FormattedBlock(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            CreateAccessTargetExpression(),
                            IdentifierName( "value" ) ) ) );
        }

        ExpressionSyntax CreateAccessTargetExpression()
        {
            return
                this._targetProperty.GetTarget(context.Compilation).IsStatic
                    ? IdentifierName( this._targetProperty.Name )
                    : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( this._targetProperty.Name ) )
                        .WithAspectReferenceAnnotation( this.AspectLayerId, AspectReferenceOrder.Previous );
        }
    }
}