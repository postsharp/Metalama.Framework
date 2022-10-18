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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal class PropertyBuilder : MemberBuilder, IPropertyBuilder, IPropertyImpl
    {
        public bool HasInitOnlySetter { get; }

        private IType _type;
        private IExpression? _initializerExpression;
        private TemplateMember<IProperty>? _initializerTemplate;

        public RefKind RefKind { get; set; }

        public virtual Writeability Writeability
            => this switch
            {
                { SetMethod: null } => Writeability.None,
                { SetMethod: { IsImplicitlyDeclared: true }, IsAutoPropertyOrField: true } => Writeability.ConstructorOnly,
                { HasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

        public bool IsAutoPropertyOrField { get; set; }

        bool? IFieldOrProperty.IsAutoPropertyOrField => this.IsAutoPropertyOrField;

        public IObjectReader InitializerTags { get; }

        public IType Type
        {
            get => this._type;
            set
            {
                this.CheckNotFrozen();

                this._type = value;
            }
        }

        public IMethodBuilder? GetMethod { get; }

        IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

        IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

        public IMethodBuilder? SetMethod { get; }

        protected virtual bool HasBaseInvoker => this.OverriddenProperty != null;

        IInvokerFactory<IFieldOrPropertyInvoker> IFieldOrProperty.Invokers => this.Invokers;

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invokers
            => new InvokerFactory<IFieldOrPropertyInvoker>(
                ( order, invokerOperator ) => new FieldOrPropertyInvoker( this, order, invokerOperator ),
                this.HasBaseInvoker );

        public IProperty? OverriddenProperty { get; set; }

        public override DeclarationKind DeclarationKind => DeclarationKind.Property;

        public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; set; } = Array.Empty<IProperty>();

        public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

        public override IMember? OverriddenMember => this.OverriddenProperty;

        public override IInjectMemberTransformation ToTransformation() => new IntroducePropertyTransformation( this.ParentAdvice, this );

        public IExpression? InitializerExpression
        {
            get => this._initializerExpression;
            set
            {
                this.CheckNotFrozen();

                this._initializerExpression = value;
            }
        }

        public TemplateMember<IProperty>? InitializerTemplate
        {
            get => this._initializerTemplate;
            set
            {
                this.CheckNotFrozen();

                this._initializerTemplate = value;
            }
        }

        public PropertyBuilder(
            INamedType targetType,
            string name,
            bool hasGetter,
            bool hasSetter,
            bool isAutoProperty,
            bool hasInitOnlySetter,
            bool hasImplicitGetter,
            bool hasImplicitSetter,
            IObjectReader initializerTags,
            Advice advice )
            : base( targetType, name, advice )
        {
            // TODO: Sanity checks.

            Invariant.Assert( hasGetter || hasSetter );
            Invariant.Assert( !(!hasSetter && hasImplicitSetter) );
            Invariant.Assert( !(!isAutoProperty && hasImplicitSetter) );

            this._type = targetType.Compilation.GetCompilationModel().Factory.GetTypeByReflectionType( typeof(object) );

            if ( hasGetter )
            {
                this.GetMethod = new AccessorBuilder( this, MethodKind.PropertyGet, hasImplicitGetter );
            }

            if ( hasSetter )
            {
                this.SetMethod = new AccessorBuilder( this, MethodKind.PropertySet, hasImplicitSetter );
            }

            this.IsAutoPropertyOrField = isAutoProperty;
            this.InitializerTags = initializerTags;
            this.HasInitOnlySetter = hasInitOnlySetter;
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

        public PropertyInfo ToPropertyInfo() => CompileTimePropertyInfo.Create( this );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };

        public override void Freeze()
        {
            base.Freeze();

            ((DeclarationBuilder?) this.GetMethod)?.Freeze();
            ((DeclarationBuilder?) this.SetMethod)?.Freeze();
        }

        protected internal virtual bool GetPropertyInitializerExpressionOrMethod(
            Advice advice,
            in MemberInjectionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            return this.GetInitializerExpressionOrMethod(
                advice,
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this.InitializerTags,
                out initializerExpression,
                out initializerMethod );
        }

        protected virtual bool GetInitializerExpressionOrMethod(
            Advice advice,
            in MemberInjectionContext context,
            out ExpressionSyntax? initializerExpression,
            out MethodDeclarationSyntax? initializerMethod )
        {
            return this.GetInitializerExpressionOrMethod(
                advice,
                context,
                this.Type,
                this.InitializerExpression,
                this.InitializerTemplate,
                this.InitializerTags,
                out initializerExpression,
                out initializerMethod );
        }
    }
}