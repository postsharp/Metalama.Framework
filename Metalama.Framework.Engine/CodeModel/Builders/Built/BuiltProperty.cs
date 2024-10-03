// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.RunTime;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders.Built;

internal class BuiltProperty : BuiltPropertyOrIndexer, IPropertyImpl
{
    public PropertyBuilder PropertyBuilder { get; }

    public BuiltProperty( PropertyBuilder builder, CompilationModel compilation, IGenericContext genericContext ) : base( compilation, genericContext )
    {
        this.PropertyBuilder = builder;
    }

    public override DeclarationBuilder Builder => this.PropertyBuilder;

    protected override NamedDeclarationBuilder NamedDeclarationBuilder => this.PropertyBuilder;

    protected override MemberOrNamedTypeBuilder MemberOrNamedTypeBuilder => this.PropertyBuilder;

    protected override MemberBuilder MemberBuilder => this.PropertyBuilder;

    protected override PropertyOrIndexerBuilder PropertyOrIndexerBuilder => this.PropertyBuilder;

    public bool? IsAutoPropertyOrField => this.PropertyBuilder.IsAutoPropertyOrField;

    [Memo]
    public IProperty? OverriddenProperty => this.MapDeclaration( this.PropertyBuilder.OverriddenProperty );

    [Memo]
    public IProperty Definition => this.Compilation.Factory.GetProperty( this.PropertyBuilder ).AssertNotNull();

    protected override IMemberOrNamedType GetDefinition() => this.Definition;

    [Memo]
    private IRef<IProperty> Ref => this.RefFactory.FromBuilt<IProperty>( this );

    IRef<IProperty> IProperty.ToRef() => this.Ref;

    IRef<IFieldOrProperty> IFieldOrProperty.ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    // TODO: When an interface is introduced, explicit implementation should appear here.
    [Memo]
    public IReadOnlyList<IProperty> ExplicitInterfaceImplementations
        => this.PropertyBuilder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => this.Compilation.Factory.Translate( i ) );

    public FieldOrPropertyInfo ToFieldOrPropertyInfo() => this.PropertyBuilder.ToFieldOrPropertyInfo();

    public bool IsRequired => this.PropertyBuilder.IsRequired;

    public IExpression? InitializerExpression => this.PropertyBuilder.InitializerExpression;

    public IFieldOrPropertyInvoker With( InvokerOptions options ) => this.PropertyBuilder.With( options );

    public IFieldOrPropertyInvoker With( object? target, InvokerOptions options = default ) => this.PropertyBuilder.With( target, options );

    public ref object? Value => ref this.PropertyBuilder.Value;

    public TypedExpressionSyntax ToTypedExpressionSyntax( ISyntaxGenerationContext syntaxGenerationContext )
        => this.PropertyBuilder.ToTypedExpressionSyntax( syntaxGenerationContext );

    bool IExpression.IsAssignable => this.Writeability != Writeability.None;

    [Memo]
    public IField? OriginalField => this.GetOriginalField();

    private IField? GetOriginalField()
    {
        using ( StackOverflowHelper.Detect() )
        {
            // Intentionally not using MapDeclaration to avoid the strong typing.

            var originalField = (IFieldBuilder?) this.PropertyBuilder.OriginalField;

            if ( originalField != null )
            {
                return this.Compilation.Factory.GetField( originalField, this.GenericContext );
            }
            else
            {
                return null;
            }
        }
    }
}