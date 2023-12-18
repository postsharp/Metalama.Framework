// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Introspection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations;

internal abstract class OverrideMemberTransformation : BaseTransformation, IInjectMemberOrNamedTypeTransformation, IOverrideDeclarationTransformation
{
    protected IObjectReader Tags { get; }

    public IMember OverriddenDeclaration { get; }

    IDeclaration IOverrideDeclarationTransformation.OverriddenDeclaration => this.OverriddenDeclaration;

    public override IDeclaration TargetDeclaration => this.OverriddenDeclaration;

    protected OverrideMemberTransformation( Advice advice, IMember overriddenDeclaration, IObjectReader tags ) : base( advice )
    {
        Invariant.Assert( advice != null! );
        Invariant.Assert( overriddenDeclaration != null! );

        this.OverriddenDeclaration = overriddenDeclaration;
        this.Tags = tags;
    }

    public abstract IEnumerable<InjectedMemberOrNamedType> GetInjectedMembers( MemberInjectionContext context );

    protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind, SyntaxGenerationContext generationContext )
    {
        ExpressionSyntax expression;

        var memberNameString =
            this.OverriddenDeclaration switch
            {
                { IsExplicitInterfaceImplementation: true } => this.OverriddenDeclaration.Name.Split( '.' ).Last(),
                _ => this.OverriddenDeclaration.Name
            };

        SimpleNameSyntax memberName;

        if ( this.OverriddenDeclaration is IGeneric { TypeParameters.Count: > 0 } generic )
        {
            memberName = GenericName( memberNameString )
                .WithTypeArgumentList(
                    TypeArgumentList( SeparatedList( generic.TypeParameters.SelectAsReadOnlyList( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
        }
        else
        {
            memberName = IdentifierName( memberNameString );
        }

        if ( !this.OverriddenDeclaration.IsStatic )
        {
            if ( this.OverriddenDeclaration.IsExplicitInterfaceImplementation )
            {
                var implementedInterfaceMember = this.OverriddenDeclaration.GetExplicitInterfaceImplementation();

                expression = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ParenthesizedExpression(
                        SyntaxFactoryEx.SafeCastExpression(
                            generationContext.SyntaxGenerator.Type( implementedInterfaceMember.DeclaringType.GetSymbol() ),
                            ThisExpression() ) ),
                    memberName );
            }
            else
            {
                expression = MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ThisExpression(),
                    memberName );
            }
        }
        else
        {
            expression =
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    generationContext.SyntaxGenerator.Type( this.OverriddenDeclaration.DeclaringType.GetSymbol() ),
                    memberName );
        }

        return expression
            .WithAspectReferenceAnnotation(
                this.ParentAdvice.AspectLayerId,
                AspectReferenceOrder.Previous,
                referenceTargetKind,
                AspectReferenceFlags.Inlineable );
    }

    public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();

    public override TransformationObservability Observability => TransformationObservability.None;

    public override FormattableString ToDisplayString() => $"Override the {this.OverriddenDeclaration.DeclarationKind} '{this.OverriddenDeclaration}'";

    public override TransformationKind TransformationKind => TransformationKind.OverrideMember;
}