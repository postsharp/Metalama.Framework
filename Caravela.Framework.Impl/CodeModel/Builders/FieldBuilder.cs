// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.DeclarationBuilders;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.CodeModel.Pseudo;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class FieldBuilder : MemberBuilder, IFieldBuilder, IFieldImpl
    {
        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public IType Type { get; set; }

        [Memo]
        public IMethod? GetMethod => new PseudoGetter( this );

        public IMethod? SetMethod => new PseudoSetter( this );

        public override bool IsExplicitInterfaceImplementation => false;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public Writeability Writeability { get; set; }

        Writeability IFieldOrProperty.Writeability => this.Writeability;

        public bool IsAutoPropertyOrField => true;

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public ExpressionSyntax? InitializerSyntax { get; set; }

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name )
        {
            this.Type = this.Compilation.Factory.GetSpecialType( SpecialType.Object );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            var field =
                FieldDeclaration(
                    List<AttributeListSyntax>(), // TODO: Attributes.
                    this.GetSyntaxModifierList(),
                    VariableDeclaration(
                        syntaxGenerator.Type( this.Type.GetSymbol() ),
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( this.Name ),
                                null,
                                this.InitializerSyntax != null
                                    ? EqualsValueClause( this.InitializerSyntax )
                                    : null ) ) ) );

            return new[] { new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
        }

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();
    }
}