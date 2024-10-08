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
    public PropertyBuilderData PropertyBuilder { get; }

    public BuiltProperty( PropertyBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.PropertyBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this.PropertyBuilder;

    protected override NamedDeclarationBuilderData NamedDeclarationBuilder => this.PropertyBuilder;

    protected override MemberOrNamedTypeBuilderData MemberOrNamedTypeBuilder => this.PropertyBuilder;

    protected override MemberBuilderData MemberBuilder => this.PropertyBuilder;

    public override bool IsExplicitInterfaceImplementation => this.ExplicitInterfaceImplementations.Count > 0;

    protected override PropertyOrIndexerBuilderData PropertyOrIndexerBuilder => this.PropertyBuilder;

    public bool? IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

    [Memo]
    public IProperty? OverriddenProperty => this.MapDeclaration( this.PropertyBuilder.OverriddenProperty );

    [Memo]
    public IProperty Definition => this.Compilation.Factory.GetProperty( this.PropertyBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    [Memo]
    private IFullRef<IProperty> Ref => this.RefFactory.FromBuilt<IProperty>( this );

    IRef<IProperty> IProperty.ToRef() => this.Ref;

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Ref;

    private protected override IFullRef<IDeclaration> ToDeclarationRef() => this.Ref;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations => this.MapDeclarationList( this.PropertyBuilder.ExplicitInterfaceImplementations );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

    public bool IsRequired => this.PropertyBuilder.IsRequired;

    public IExpression? InitializerExpression => this.PropertyBuilder.InitializerExpression;

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
            // Intentionally not using MapDeclaration to avoid the strong typing.

            return this.PropertyBuilder.GetOriginalField( this.Compilation, this.GenericContext );
        }
    }
}