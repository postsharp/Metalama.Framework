// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Transformations
{
    internal abstract class OverrideMemberTransformation : BaseTransformation, INonObservableTransformation, IIntroduceMemberTransformation,
                                                           IOverriddenDeclaration
    {
        protected IObjectReader Tags { get; }

        public IMember OverriddenDeclaration { get; }

        IDeclaration IOverriddenDeclaration.OverriddenDeclaration => this.OverriddenDeclaration;

        public override IDeclaration TargetDeclaration => this.OverriddenDeclaration;

        protected OverrideMemberTransformation( Advice advice, IMember overriddenDeclaration, IObjectReader tags ) : base( advice )
        {
            Invariant.Assert( advice != null! );
            Invariant.Assert( overriddenDeclaration != null! );

            this.OverriddenDeclaration = overriddenDeclaration;
            this.Tags = tags;
        }

        public abstract IEnumerable<IntroducedMember> GetIntroducedMembers( MemberIntroductionContext context );

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

            if ( this.OverriddenDeclaration is IGeneric generic && generic.TypeParameters.Count > 0 )
            {
                memberName = GenericName( memberNameString )
                    .WithTypeArgumentList( TypeArgumentList( SeparatedList( generic.TypeParameters.Select( p => (TypeSyntax) IdentifierName( p.Name ) ) ) ) );
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
                    this.ParentAdvice.AspectLayerId,
                    AspectReferenceOrder.Base,
                    referenceTargetKind,
                    flags: AspectReferenceFlags.Inlineable );
        }

        public InsertPosition InsertPosition => this.OverriddenDeclaration.ToInsertPosition();

        public override string ToString() => $"Override {this.OverriddenDeclaration} by {this.ParentAdvice.AspectLayerId}";
    }
}