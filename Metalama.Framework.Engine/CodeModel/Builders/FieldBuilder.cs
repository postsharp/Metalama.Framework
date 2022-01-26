// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class FieldBuilder : FieldOrPropertyBuilder, IFieldBuilder, IFieldImpl
    {
        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public override IType Type { get; set; }

        [Memo]
        public override IMethodBuilder? GetMethod => new AccessorBuilder( this, MethodKind.PropertyGet );

        [Memo]
        public override IMethodBuilder? SetMethod => new AccessorBuilder( this, MethodKind.PropertySet );

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        protected override IInvokerFactory<IFieldOrPropertyInvoker> FieldOrPropertyInvokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public override Writeability Writeability { get; set; }

        Writeability IFieldOrProperty.Writeability => this.Writeability;

        public override bool IsAutoPropertyOrField => true;

        public override InsertPosition InsertPosition
            => new(
                InsertPositionRelation.Within,
                (MemberDeclarationSyntax) ((NamedType) this.DeclaringType).Symbol.GetPrimaryDeclaration().AssertNotNull() );

        public override IExpression? InitializerExpression { get; set; }

        public TemplateMember<IField> InitializerTemplate { get; set; }

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name )
            : base( parentAdvice, targetType, name )
        {
            this.Type = this.Compilation.Factory.GetSpecialType( SpecialType.Object );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            // If template fails to expand, we will still generate the field, albeit without the initializer.
            _ = this.GetInitializerExpressionOrMethod( context, this.InitializerExpression, this.InitializerTemplate, out var initializerExpression, out var initializerMethod );

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
                                initializerExpression != null
                                    ? EqualsValueClause( initializerExpression! )
                                    : null ) ) ) );

            if ( initializerMethod != null )
            {
                return new[] 
                {
                    new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ),
                    new IntroducedMember( this, initializerMethod, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.InitializerMethod, this ),
                };
            }
            else
            {
                return new[] { new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
        }

        public override IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public override IEnumerable<IMethod> Accessors
        {
            get
            {
                if ( this.GetMethod != null )
                {
                    yield return this.GetMethod;
                }

                if ( this.SetMethod != null )
                {
                    yield return this.SetMethod;
                }
            }
        }

        public FieldInfo ToFieldInfo() => throw new NotImplementedException();

        public override FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();
    }
}