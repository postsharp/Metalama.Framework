// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.RunTime;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;
using RefKind = Metalama.Framework.Code.RefKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal class PropertyBuilder : MemberBuilder, IPropertyBuilder, IPropertyImpl
{
    private readonly List<IAttributeData> _fieldAttributes;
    private IType _type;
    private IExpression? _initializerExpression;
    private TemplateMember<IProperty>? _initializerTemplate;

    public bool HasInitOnlySetter { get; private set; }

    public RefKind RefKind { get; set; }

    public IReadOnlyList<IAttributeData> FieldAttributes => this._fieldAttributes;

    public virtual Writeability Writeability
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

    public IType Type
    {
        get => this._type;
        set
        {
            this.CheckNotFrozen();

            this._type = this.Translate( value );
        }
    }

    public IMethodBuilder? GetMethod { get; }

    IMethod? IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

    IMethod? IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

    public IMethodBuilder? SetMethod { get; }

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

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this, syntaxGenerationContext: ((SyntaxSerializationContext) syntaxGenerationContext).SyntaxGenerationContext )
            .GetTypedExpressionSyntax();

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
        this._fieldAttributes = new List<IAttributeData>();
    }

    public void AddFieldAttribute( IAttributeData attributeData )
    {
        this._fieldAttributes.Add( attributeData );
    }

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

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

    public bool IsRequired { get; set; }

    public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = new[] { interfaceProperty };

    public override void Freeze()
    {
        base.Freeze();

        ((DeclarationBuilder?) this.GetMethod)?.Freeze();
        ((DeclarationBuilder?) this.SetMethod)?.Freeze();
    }

    protected internal virtual bool GetPropertyInitializerExpressionOrMethod(
        Advice advice,
        MemberInjectionContext context,
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

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;
}