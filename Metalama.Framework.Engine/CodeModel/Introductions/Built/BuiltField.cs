// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
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
    public FieldBuilderData FieldBuilder { get; }

    public BuiltField( FieldBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.FieldBuilder = builder;
    }

    // DeclarationKind is always a field even if the underlying builder may be a PromotedField i.e. a property.
    public override DeclarationKind DeclarationKind => DeclarationKind.Field;

    public override DeclarationBuilderData BuilderData => this.FieldBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.FieldBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.FieldBuilder;

    protected override MemberBuilderData MemberBuilder => this.FieldBuilder;

    public override bool IsExplicitInterfaceImplementation => throw new NotImplementedException();

    public Writeability Writeability => this.FieldBuilder.Writeability;

    public bool? IsAutoPropertyOrField => true;

    public IType Type => this.MapType( this.FieldBuilder.Type );

    public RefKind RefKind => this.FieldBuilder.RefKind;

    [Memo]
    public IMethod GetMethod => new BuiltAccessor( this, this.FieldBuilder.GetMethod! );

    [Memo]
    public IMethod SetMethod => new BuiltAccessor( this, this.FieldBuilder.SetMethod! );

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Ref;

    [Memo]
    public IProperty? OverridingProperty => this.MapDeclaration( this.FieldBuilder.OverridingProperty );

    [Memo]
    private IRef<IField> Ref => this.RefFactory.FromBuilt<IField>( this );

    public IRef<IField> ToRef() => this.Ref;

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired => this.FieldBuilder.IsRequired;

    public IExpression? InitializerExpression => this.FieldBuilder.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

    public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this )
            .ToTypedExpressionSyntax( syntaxGenerationContext );

    public FieldInfo ToFieldInfo() => CompileTimeFieldInfo.Create( this );

    public TypedConstant? ConstantValue => this.FieldBuilder.ConstantValue;

    [Memo]
    public IField Definition => this.Compilation.Factory.GetField( this.FieldBuilder ).AssertNotNull();

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