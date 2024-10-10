// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.Invokers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal class BuiltProperty : BuiltPropertyOrIndexer, IPropertyImpl
{
    public PropertyBuilderData PropertyBuilderData { get; }

    public BuiltProperty( PropertyBuilderData builderData, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.PropertyBuilderData = builderData;
    }

    public override DeclarationBuilderData BuilderData => this.PropertyBuilderData;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilderData => this.PropertyBuilderData;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilderData => this.PropertyBuilderData;

    protected override MemberBuilderData MemberBuilderData => this.PropertyBuilderData;

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    protected override PropertyOrIndexerBuilderData PropertyOrIndexerBuilderData => this.PropertyBuilderData;

    public bool? IsAutoPropertyOrField => this.PropertyBuilderData.IsAutoPropertyOrField;

    [Memo]
    public IProperty? OverriddenProperty => this.MapDeclaration( this.PropertyBuilderData.OverriddenProperty );

    [Memo]
    public IProperty Definition => this.Compilation.Factory.GetProperty( this.PropertyBuilderData ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    [Memo]
    private IFullRef<IProperty> Ref => this.RefFactory.FromBuilt<IProperty>( this );

    IRef<IProperty> IProperty.ToRef() => this.Ref;

    public override IFullRef<IMember> ToMemberFullRef() => this.Ref;

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToFullDeclarationRef() => this.Ref;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.MapDeclarationList( this.PropertyBuilderData.ExplicitInterfaceImplementations );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired => this.PropertyBuilderData.IsRequired;

    public IExpression? InitializerExpression => this.PropertyBuilderData.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => new FieldOrPropertyInvoker( this, options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => new FieldOrPropertyInvoker( this, options, target );

    public ref object? Value => ref new FieldOrPropertyInvoker( this ).Value;

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => new FieldOrPropertyInvoker( this )
            .ToTypedExpressionSyntax( syntaxGenerationContext );

    [Memo]
    public IField? OriginalField => this.GetOriginalField();

    private IField? GetOriginalField()
    {
        using ( StackOverflowHelper.Detect() )
        {
            return this.MapDeclaration( this.PropertyBuilderData.OriginalField );
        }
    }
}