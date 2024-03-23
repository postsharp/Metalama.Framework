// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.SyntaxGeneration;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

/// <summary>
/// Represents a property override, which redirects to accessors of another property without requiring template expansion.
/// </summary>
internal sealed class RedirectPropertyTransformation : OverrideMemberTransformation
{
    private readonly IProperty _targetProperty;

    private new IProperty OverriddenDeclaration => (IProperty) base.OverriddenDeclaration;

    public RedirectPropertyTransformation( Advice advice, IProperty overriddenDeclaration, IProperty targetProperty )
        : base( advice, overriddenDeclaration, ObjectReader.Empty )
    {
        this._targetProperty = targetProperty;
    }

    public override IEnumerable<InjectedMember> GetInjectedMembers( MemberInjectionContext context )
    {
        return new[]
        {
            new InjectedMember(
                this,
                PropertyDeclaration(
                    List<AttributeListSyntax>(),
                    this.OverriddenDeclaration.GetSyntaxModifierList(),
                    context.SyntaxGenerator.PropertyType( this.OverriddenDeclaration )
                        .WithOptionalTrailingTrivia( ElasticSpace, context.SyntaxGenerationContext.Options ),
                    null,
                    Identifier(
                        context.InjectionNameProvider.GetOverrideName(
                            this.OverriddenDeclaration.DeclaringType,
                            this.ParentAdvice.AspectLayerId,
                            this.OverriddenDeclaration ) ),
                    AccessorList( List( GetAccessors() ) ),
                    null,
                    null ),
                this.ParentAdvice.AspectLayerId,
                InjectedMemberSemantic.Override,
                this.OverriddenDeclaration )
        };

        IReadOnlyList<AccessorDeclarationSyntax> GetAccessors()
        {
            return new[]
                {
                    this.OverriddenDeclaration.GetMethod != null
                        ? AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.GetMethod.GetSyntaxModifierList(),
                            CreateGetterBody(),
                            null )
                        : null,
                    this.OverriddenDeclaration.SetMethod != null
                        ? AccessorDeclaration(
                            this.OverriddenDeclaration.Writeability != Writeability.InitOnly
                                ? SyntaxKind.SetAccessorDeclaration
                                : SyntaxKind.InitAccessorDeclaration,
                            List<AttributeListSyntax>(),
                            this.OverriddenDeclaration.SetMethod.GetSyntaxModifierList(),
                            CreateSetterBody(),
                            null )
                        : null
                }.Where( a => a != null )
                .AssertNoneNull()
                .ToArray();
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
                this._targetProperty.IsStatic
                    ? IdentifierName( this._targetProperty.Name )
                    : MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, ThisExpression(), IdentifierName( this._targetProperty.Name ) )
                        .WithAspectReferenceAnnotation(
                            this.OverriddenDeclaration.ToSerializableId(),
                            this.ParentAdvice.AspectLayerId,
                            AspectReferenceOrder.Previous );
        }
    }
}