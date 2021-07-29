// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
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

        public OverriddenMember( Advice advice, IMember overriddenDeclaration )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );

            this.Advice = advice;
            this.OverriddenDeclaration = overriddenDeclaration;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context );

        protected ExpressionSyntax CreateMemberAccessExpression( AspectReferenceTargetKind referenceTargetKind )
        {
            ExpressionSyntax expression;

            if ( !this.OverriddenDeclaration.IsStatic )
            {
                if ( this.OverriddenDeclaration.IsExplicitInterfaceImplementation )
                {
                    var implementedInterfaceMember = this.OverriddenDeclaration.GetExplicitInterfaceImplementation();

                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ParenthesizedExpression(
                            CastExpression(
                                LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( implementedInterfaceMember.DeclaringType.GetSymbol() ),
                                ThisExpression() ) ),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
                }
                else
                {
                    expression = MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
                }
            }
            else
            {
                expression =
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        LanguageServiceFactory.CSharpSyntaxGenerator.TypeExpression( this.OverriddenDeclaration.DeclaringType.GetSymbol() ),
                        IdentifierName( this.OverriddenDeclaration.Name ) );
            }

            return expression
                .WithAspectReferenceAnnotation(
                    this.Advice.AspectLayerId,
                    AspectReferenceOrder.Base,
                    referenceTargetKind,
                    flags: AspectReferenceFlags.Inlineable );
        }

        public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();
    }
}