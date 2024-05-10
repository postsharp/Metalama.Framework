// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class PropertyBuilder : PropertyOrIndexerBuilder, IPropertyBuilder, IPropertyImpl
{
    private readonly List<IAttributeData> _fieldAttributes;
    private IExpression? _initializerExpression;
    private TemplateMember<IProperty>? _initializerTemplate;

    public IReadOnlyList<IAttributeData> FieldAttributes => this._fieldAttributes;

    public override Writeability Writeability
    {
        get
            => this switch
            {
                { SetMethod: null } => Writeability.None,
                { SetMethod.IsImplicitlyDeclared: true, IsAutoPropertyOrField: true } => Writeability.ConstructorOnly,
                { HasInitOnlySetter: true } => Writeability.InitOnly,
                _ => Writeability.All
            };

        set
            => this.HasInitOnlySetter = (this, value) switch
            {
                ({ SetMethod: not null }, Writeability.All) => false,
                ({ SetMethod: not null }, Writeability.InitOnly) => true,
                _ => throw new InvalidOperationException(
                    $"Writeability can only be set for non-auto properties with a setter to either {Writeability.InitOnly} or {Writeability.All}." )
            };
    }

    public bool IsAutoPropertyOrField { get; }

    bool? IFieldOrProperty.IsAutoPropertyOrField => this.IsAutoPropertyOrField;

    protected IObjectReader InitializerTags { get; }

    public IProperty? OverriddenProperty { get; set; }

    IProperty IProperty.Definition => this;

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;

    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IProperty>();

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public override IMember? OverriddenMember => this.OverriddenProperty;

    public virtual IInjectMemberTransformation ToTransformation() => new IntroducePropertyTransformation( this.ParentAdvice, this );

    public IExpression? InitializerExpression
    {
        get => this._initializerExpression;
        set
        {
            this.CheckNotFrozen();

            this._initializerExpression = value;
        }
    }

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

    public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this )
            .ToTypedExpressionSyntax( syntaxGenerationContext );

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
        Advice advice,
        INamedType targetType,
        string name,
        bool hasGetter,
        bool hasSetter,
        bool isAutoProperty,
        bool hasInitOnlySetter,
        bool hasImplicitGetter,
        bool hasImplicitSetter,
        IObjectReader initializerTags )
        : base( advice, targetType, name, hasGetter, hasSetter, hasImplicitGetter, hasImplicitSetter )
    {
        // TODO: Sanity checks.

        Invariant.Assert( hasGetter || hasSetter );
        Invariant.Assert( !(!hasSetter && hasImplicitSetter) );
        Invariant.Assert( !(!isAutoProperty && hasImplicitSetter) );

        this.IsAutoPropertyOrField = isAutoProperty;
        this.InitializerTags = initializerTags;
        this.HasInitOnlySetter = hasInitOnlySetter;
        this._fieldAttributes = new List<IAttributeData>();
    }

    public void AddFieldAttribute( IAttributeData attributeData ) => this._fieldAttributes.Add( attributeData );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired { get; set; }

    public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };

    protected internal virtual bool GetPropertyInitializerExpressionOrMethod(
        Advice advice,
        MemberInjectionContext context,
        out ExpressionSyntax? initializerExpression,
        out MethodDeclarationSyntax? initializerMethod )
        => this.GetInitializerExpressionOrMethod(
            advice,
            context,
            this.Type,
            this.InitializerExpression,
            this.InitializerTemplate,
            this.InitializerTags,
            out initializerExpression,
            out initializerMethod );
}