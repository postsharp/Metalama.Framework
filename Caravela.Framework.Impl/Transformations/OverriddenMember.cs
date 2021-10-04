// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class OverriddenMember : INonObservableTransformation, IMemberIntroduction, IOverriddenDeclaration
    {
        public Advice Advice { get; }

        public IMember OverriddenDeclaration { get; }

        IDeclaration IOverriddenDeclaration.OverriddenDeclaration => this.OverriddenDeclaration;

        // TODO: Temporary
        public SyntaxTree TargetSyntaxTree
            => this.OverriddenDeclaration is ISyntaxTreeTransformation introduction
                ? introduction.TargetSyntaxTree
                : ((NamedType) this.OverriddenDeclaration.DeclaringType).Symbol.GetPrimarySyntaxReference().AssertNotNull().SyntaxTree;

        protected OverriddenMember( Advice advice, IMember overriddenDeclaration )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind, SyntaxGenerationContext generationContext )
        {
            ExpressionSyntax expression;

            SimpleNameSyntax memberName = IdentifierName( this.OverriddenDeclaration.Name );

            if ( this.OverriddenDeclaration is IGeneric generic && generic.TypeParameters.Count > 0 )
            {
                memberName = GenericName( this.OverriddenDeclaration.Name )
                    .WithTypeArgumentList( TypeArgumentList( SeparatedList( generic.TypeParameters.Select( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
            }

            if ( !this.OverriddenDeclaration.IsStatic )
            {
                if ( this.OverriddenDeclaration.IsExplicitInterfaceImplementation )
                {
                    var implementedInterfaceMember = this.OverriddenDeclaration.GetExplicitInterfaceImplementation();

                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastExpression(
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
                    this.Advice.AspectLayerId,
                    AspectReferenceOrder.Base,
                    referenceTargetKind,
                    flags: AspectReferenceFlags.Inlineable );
        }

        public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();

        public override string ToString() => $"Override {this.OverriddenDeclaration}";
    }
}