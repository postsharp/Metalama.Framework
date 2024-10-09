// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.AdviceImpl.Introduction;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal sealed class FieldBuilder : MemberBuilder, IFieldBuilder, IFieldImpl
{
    private IType _type;
    private Writeability _writeability;

    public IObjectReader InitializerTags { get; }

    public override DeclarationKind DeclarationKind => DeclarationKind.Field;

    public IType Type
    {
        get => this._type;
        set => this._type = this.Translate( value );
    }

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

    IMethod IFieldOrPropertyOrIndexer.GetMethod => this.GetMethod;

    IMethod IFieldOrPropertyOrIndexer.SetMethod => this.SetMethod;

    [Memo]
    public AccessorBuilder GetMethod => new( this, MethodKind.PropertyGet, true );

    [Memo]
    public AccessorBuilder SetMethod => new( this, MethodKind.PropertySet, true );

    public override bool IsExplicitInterfaceImplementation => false;

    public override IMember? OverriddenMember => null;

    public IProperty? OverridingProperty => null;

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Immutable.ToRef();

    protected override IFullRef<IMember> ToMemberFullRef() => this.Immutable.ToRef();

    protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Immutable.ToRef();

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.Immutable.ToRef();

    IRef<IField> IField.ToRef() => this.Immutable.ToRef();

    public IInjectMemberTransformation ToTransformation()
    {
        return new IntroduceFieldTransformation( this.AspectLayerInstance, this.Immutable );
    }

    public Writeability Writeability
    {
        get => this._writeability;
        set
        {
            if ( value == Writeability.InitOnly )
            {
                throw new InvalidOperationException(
                    $"Writeability for fields can only be set to {Writeability.All} (no modifier), {Writeability.ConstructorOnly} (readonly) or {Writeability.None} (const)." );
            }

            this._writeability = value;
        }
    }

    public bool? IsAutoPropertyOrField => true;

    public IExpression? InitializerExpression { get; set; }

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

    public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this )
            .ToTypedExpressionSyntax( syntaxGenerationContext );

    public TemplateMember<IField>? InitializerTemplate { get; set; }

    public FieldBuilder( AspectLayerInstance aspectLayerInstance, INamedType targetType, string name, IObjectReader initializerTags )
        : base( targetType, name, aspectLayerInstance )
    {
        this.InitializerTags = initializerTags;
        this._type = this.Compilation.Cache.SystemObjectType;
    }

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors
    {
        get
        {
            yield return this.GetMethod;
            yield return this.SetMethod;
        }
    }

    public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

    // TODO: If we support introducing const fields, implement ConstantValue.
    public TypedConstant? ConstantValue => null;

    IField IField.Definition => this;

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired { get; set; }

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;

    [Memo]
    public FieldBuilderData Immutable => new( this.AssertFrozen(), this.ContainingDeclaration.ToFullRef<INamedType>() );

    public override void Freeze()
    {
        base.Freeze();
        this.GetMethod.Freeze();
        this.SetMethod.Freeze();
    }
}