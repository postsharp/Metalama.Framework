// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal class PropertyBuilder : PropertyOrIndexerBuilder, IPropertyBuilder, IPropertyImpl
{
    private readonly List<IAttributeData> _fieldAttributes;
    private IExpression? _initializerExpression;

    // private TemplateMember<IProperty>? _initializerTemplate;

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

    public IProperty? OverriddenProperty { get; set; }

    public IProperty Definition => this;

    public IField? OriginalField { get; set; }

    public override DeclarationKind DeclarationKind => DeclarationKind.Property;

    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; private set; } = Array.Empty<IProperty>();

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    public override IMember? OverriddenMember => this.OverriddenProperty;

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

    public PropertyBuilder(
        AspectLayerInstance aspectLayerInstance,
        INamedType targetType,
        string name,
        bool hasGetter,
        bool hasSetter,
        bool isAutoProperty,
        bool hasInitOnlySetter,
        bool hasImplicitGetter,
        bool hasImplicitSetter )
        : base( aspectLayerInstance, targetType, name, hasGetter, hasSetter, hasImplicitGetter, hasImplicitSetter )
    {
        // TODO: Sanity checks.

        Invariant.Assert( hasGetter || hasSetter );
        Invariant.Assert( !(!hasSetter && hasImplicitSetter) );
        Invariant.Assert( !(!isAutoProperty && hasImplicitSetter) );

        this.IsAutoPropertyOrField = isAutoProperty;
        this.HasInitOnlySetter = hasInitOnlySetter;
        this._fieldAttributes = [];
    }

    public void AddFieldAttribute( IAttributeData attributeData ) => this._fieldAttributes.Add( attributeData );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired { get; set; }

    public void SetExplicitInterfaceImplementation( IProperty interfaceProperty ) => this.ExplicitInterfaceImplementations = [interfaceProperty];

    IRef<IProperty> IProperty.ToRef() => this.Immutable.ToRef();

    protected override IFullRef<IMember> ToMemberFullRef() => this.Immutable.ToRef();

    protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Immutable.ToRef();

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Immutable.ToRef();

    public new IFullRef<IProperty> ToRef() => this.Immutable.ToRef();

    [Memo]
    public PropertyBuilderData Immutable => new( this.AssertFrozen(), this.DeclaringType.ToFullRef() );

    public bool? IsDesignTimeObservableOverride { get; set; }

    public override bool IsDesignTimeObservable => this.IsDesignTimeObservableOverride ?? base.IsDesignTimeObservable;
}