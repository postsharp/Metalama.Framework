﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Reflection;
using MethodKind = Metalama.Framework.Code.MethodKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltField : BuiltMember, IFieldImpl
{
    public FieldBuilder FieldBuilder { get; }

    public BuiltField( CompilationModel compilation, FieldBuilder builder ) : base( compilation )
    {
        this.FieldBuilder = builder;
    }

    public override DeclarationBuilder Builder => this.FieldBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this.FieldBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.FieldBuilder;

    protected override MemberBuilder MemberBuilder => this.FieldBuilder;

    public Writeability Writeability => this.FieldBuilder.Writeability;

    public bool? IsAutoPropertyOrField => this.FieldBuilder.IsAutoPropertyOrField;

    public IType Type => this.FieldBuilder.Type;

    public RefKind RefKind => this.FieldBuilder.RefKind;

    [Memo]
    public IMethod GetMethod => new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.GetMethod );

    [Memo]
    public IMethod SetMethod => new BuiltAccessor( this, (AccessorBuilder) this.FieldBuilder.SetMethod );

    IRef<IField> IField.ToRef() => this.FieldBuilder.BoxedRef;

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.FieldBuilder.BoxedRef;

    IRef<IFieldOrPropertyOrIndexer> IFieldOrPropertyOrIndexer.ToRef() => this.FieldBuilder.BoxedRef;

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.FieldBuilder.ToFieldOrPropertyInfo();

    public bool IsRequired => this.FieldBuilder.IsRequired;

    public IExpression? InitializerExpression => this.FieldBuilder.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => this.FieldBuilder.With( options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => this.FieldBuilder.With( target, options );

    public ref object? Value => ref this.FieldBuilder.Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => this.FieldBuilder.ToTypedExpressionSyntax( syntaxGenerationContext );

    public FieldInfo ToFieldInfo() => this.FieldBuilder.ToFieldInfo();

    public TypedConstant? ConstantValue => this.FieldBuilder.ConstantValue;

    IField IField.Definition => this;

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