// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.Linking;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Transformations
{
    /// <summary>
    /// Represents a method override, which redirects to another method without requiring template expansion.
    /// </summary>
    internal class RedirectedMethod : OverriddenMember
    {
        public new IMethod OverriddenDeclaration => (IMethod) base.OverriddenDeclaration;

        public IMethod TargetMethod { get; }

        public RedirectedMethod( Advice advice, IMethod overriddenDeclaration, IMethod targetMethod, AspectLinkerOptions? linkerOptions = null )
            : base( advice, overriddenDeclaration, linkerOptions )
        {
            Invariant.Assert( targetMethod != null );

            this.TargetMethod = targetMethod;
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var body =
                Block(
                    this.OverriddenDeclaration.ReturnType != this.OverriddenDeclaration.Compilation.TypeFactory.GetTypeByReflectionType( typeof(void) )
                        ? ReturnStatement( GetInvocationExpression() )
                        : ExpressionStatement( GetInvocationExpression() ) );

            return new[]
            {
                new IntroducedMember(
                    this,
                    MethodDeclaration(
                        List<AttributeListSyntax>(),
                        this.OverriddenDeclaration.GetSyntaxModifierList(),
                        this.OverriddenDeclaration.GetSyntaxReturnType(),
                        null,
                        Identifier( context.IntroductionNameProvider.GetOverrideName( this.Advice.AspectLayerId, this.OverriddenDeclaration ) ),
                        this.OverriddenDeclaration.GetSyntaxTypeParameterList(),
                        this.OverriddenDeclaration.GetSyntaxParameterList(),
                        this.OverriddenDeclaration.GetSyntaxConstraintClauses(),
                        body,
                        null ),
                    this.Advice.AspectLayerId,
                    IntroducedMemberSemantic.Override,
                    this.LinkerOptions,
                    this.OverriddenDeclaration )
            };

            ExpressionSyntax GetInvocationExpression()
            {
                return
                    InvocationExpression(
                            GetInvocationTargetExpression(),
                            ArgumentList( SeparatedList( this.OverriddenDeclaration.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) )
                        .AddLinkerAnnotation( new LinkerAnnotation( this.Advice.AspectLayerId, LinkingOrder.Default ) );
            }

            ExpressionSyntax GetInvocationTargetExpression()
            {
                return
                    this.OverriddenDeclaration.IsStatic
                        ? IdentifierName( this.OverriddenDeclaration.Name )
                        : MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName( this.OverriddenDeclaration.Name ) );
            }
        }
    }
}