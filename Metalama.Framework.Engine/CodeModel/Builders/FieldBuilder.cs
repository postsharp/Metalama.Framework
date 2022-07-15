// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class FieldBuilder : MemberBuilder, IFieldBuilder, IFieldImpl
    {
        private readonly IObjectReader _initializerTags;

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public IType Type { get; set; }

        public override bool IsImplicit => false;

        [Memo]
        public IMethod? GetMethod => new AccessorBuilder( this, MethodKind.PropertyGet, true );

        [Memo]
        public IMethod? SetMethod => new AccessorBuilder( this, MethodKind.PropertySet, true );

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public Writeability Writeability { get; set; }

        public bool IsAutoPropertyOrField => true;

        public IExpression? InitializerExpression { get; set; }

        public TemplateMember<IField> InitializerTemplate { get; set; }

        public FieldBuilder( Advice parentAdvice, INamedType targetType, string name, IObjectReader initializerTags )
            : base( parentAdvice, targetType, name )
        {
            this._initializerTags = initializerTags;
            this.Type = this.Compilation.Factory.GetSpecialType( SpecialType.Object );
        }

        public override IEnumerable<IntroducedMember> GetIntroducedMembers( in MemberIntroductionContext context )
        {
            var syntaxGenerator = context.SyntaxGenerationContext.SyntaxGenerator;

            // If template fails to expand, we will still generate the field, albeit without the initializer.
            _ = this.GetInitializerExpressionOrMethod(
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this._initializerTags,
                out var initializerExpression,
                out var initializerMethod );

            // If we are introducing a field into a struct, it must have an explicit default value.
            if ( initializerExpression == null && this.DeclaringType.TypeKind is TypeKind.Struct or TypeKind.RecordStruct )
            {
                initializerExpression = SyntaxFactoryEx.Default;
            }

            var field =
                FieldDeclaration(
                    this.GetAttributeLists( context ),
                    this.GetSyntaxModifierList(),
                    VariableDeclaration(
                        syntaxGenerator.Type( this.Type.GetSymbol() ),
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( this.Name ),
                                null,
                                initializerExpression != null
                                    ? EqualsValueClause( initializerExpression )
                                    : null ) ) ) );

            if ( initializerMethod != null )
            {
                return new[]
                {
                    new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ),
                    new IntroducedMember( this, initializerMethod, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.InitializerMethod, this )
                };
            }
            else
            {
                return new[] { new IntroducedMember( this, field, this.ParentAdvice.AspectLayerId, IntroducedMemberSemantic.Introduction, this ) };
            }
        }

        public IMethod? GetAccessor( MethodKind methodKind )
            => methodKind switch
            {
                MethodKind.PropertyGet => this.GetMethod,
                MethodKind.PropertySet => this.SetMethod,
                _ => null
            };

        public IEnumerable<IMethod> Accessors
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

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => throw new NotImplementedException();
    }
}