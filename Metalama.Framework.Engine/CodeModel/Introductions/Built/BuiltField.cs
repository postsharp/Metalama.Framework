// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Source;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltField : BuiltMember, IFieldImpl
{
    public FieldBuilderData FieldBuilderData { get; }

    public BuiltField( FieldBuilderData builderData, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.FieldBuilderData = builderData;
    }

    // DeclarationKind is always a field even if the underlying builder may be a PromotedField i.e. a property.
    public override DeclarationKind DeclarationKind => DeclarationKind.Field;

    public override DeclarationBuilderData BuilderData => this.FieldBuilderData;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.FieldBuilderData;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.FieldBuilderData;

    protected override MemberBuilderData MemberBuilder => this.FieldBuilderData;

    public override bool IsExplicitInterfaceImplementation => throw new NotImplementedException();

    public Writeability Writeability => this.FieldBuilderData.Writeability;

    public bool? IsAutoPropertyOrField => true;

    public IType Type => this.MapType( this.FieldBuilderData.Type );

    public RefKind RefKind => this.FieldBuilderData.RefKind;

    [Memo]
    public IMethod GetMethod => new BuiltAccessor( this, this.FieldBuilderData.GetMethod! );

    [Memo]
    public IMethod SetMethod => new BuiltAccessor( this, this.FieldBuilderData.SetMethod! );

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Ref;

    [Memo]
    public IProperty? OverridingProperty => FieldHelper.GetOverridingProperty( this );

    [Memo]
    private IFullRef<IField> Ref => this.RefFactory.FromBuilt<IField>( this );

    public IRef<IField> ToRef() => this.Ref;

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired => this.FieldBuilderData.IsRequired;

    public IExpression? InitializerExpression => this.FieldBuilderData.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

    public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this )
            .ToTypedExpressionSyntax( syntaxGenerationContext );

    public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

    public TypedConstant? ConstantValue => this.FieldBuilderData.ConstantValue;

    [Memo]
    public IField Definition => this.Compilation.Factory.GetField( this.FieldBuilderData ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    public IMethod? GetAccessor( MethodKind methodKind ) => this.GetAccessorImpl( methodKind );

    public IEnumerable<IMethod> Accessors
    {
        get
        {
            yield return this.GetMethod;
            yield return this.SetMethod;
        }
    }

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;
}