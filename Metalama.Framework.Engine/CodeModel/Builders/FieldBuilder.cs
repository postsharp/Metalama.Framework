// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class FieldBuilder : MemberBuilder, IFieldBuilder, IFieldImpl
    {
        public IObjectReader InitializerTags { get; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public IType Type { get; set; }

        public RefKind RefKind
        {
            get => RefKind.None;
            set
            {
                if ( value != RefKind.None )
                {
                    throw new InvalidOperationException( $"Changing the {nameof(this.RefKind)} property is not supported." );
                }
            }
        }

        [Memo]
        public IMethod? GetMethod => new AccessorBuilder( this, MethodKind.PropertyGet, true );

        [Memo]
        public IMethod? SetMethod => new AccessorBuilder( this, MethodKind.PropertySet, true );

        public override bool IsExplicitInterfaceImplementation => false;

        public override IMember? OverriddenMember => null;

        public override IInjectMemberTransformation ToTransformation() => new IntroduceFieldTransformation( this.ParentAdvice, this );

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>( ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ), false );

        public Writeability Writeability { get; set; }

        public bool? IsAutoPropertyOrField => true;

        public IExpression? InitializerExpression { get; set; }

        public TemplateMember<IField>? InitializerTemplate { get; set; }

        public FieldBuilder( Advice advice, INamedType targetType, string name, IObjectReader initializerTags )
            : base( targetType, name, advice )
        {
            this.InitializerTags = initializerTags;
            this.Type = this.Compilation.Factory.GetSpecialType( SpecialType.Object );
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

        public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public bool IsRequired { get; set; }
    }
}